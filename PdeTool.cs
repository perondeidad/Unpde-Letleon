using System.Text;
using static Unpde.DataType;
using static Program;

namespace Unpde {

    /// <summary>
    /// PDE工具类
    /// </summary>
    public static class PdeTool {

        /// <summary>
        /// PDE大小
        /// </summary>
        private static uint PDESIZE;

        /// <summary>
        /// 获取PDE大小
        /// </summary>
        public static uint PdeSize {
            get { return PDESIZE; }
        }

        /// <summary>
        /// PDEKEY
        /// </summary>
        private static byte[] PDEKEY = [];

        /// <summary>
        /// 当前PDE文件全名 
        /// </summary>
        private static PdeNames THISPDENAME = new() { Name = "", FullName = "" };

        /// <summary>
        /// 获取当前PDE文件名称
        /// </summary>
        public static PdeNames ThisPdeName {
            get { return THISPDENAME; }
        }

        /// <summary>
        /// 初始化PDE工具类
        /// </summary>
        /// <param name="PdeList">当前目录下的PDE资源文件</param>
        public static void Init(PdeNames PdeList) {
            Console.WriteLine(" ！初始化PDE工具类");

            // 当前目录下的PDE资源文件
            THISPDENAME = PdeList;

            // 读取PDEFILE文件,如果不存在则退出
            if (!File.Exists(THISPDENAME.FullName)) {
                Console.WriteLine(" ！目录下没有找到 " + THISPDENAME + " 文件");
                // 退出程序
                Environment.Exit(0);
            }

            // 判断是否存在 PdeNames.Name 目录，不存在则创建
            if (!Directory.Exists(PdeList.Name)) {
                Directory.CreateDirectory(PdeList.Name);
            }

            // 获取PDEKEY
            if (PDEKEY.Length == 0) {
                PDEKEY = PdeKey.PDEKEY();
            }

            // 获取PDESIZE
            using var pdeFileStream = File.OpenRead(THISPDENAME.FullName);
            PDESIZE = (uint)pdeFileStream.Length;

            Console.WriteLine(" √ 成功初始化PDE工具类");
        }

        /// <summary>
        /// 获取文件或文件夹偏移信息
        /// </summary>
        /// <param name="data">初次解密后的数据</param>
        /// <param name="BlockOffset">数据块在PDE文件中的偏移值</param>
        /// <returns>文件或文件夹的偏移信息数组 </returns>
        public static List<HexOffsetInfo> GetOffsetInfo(byte[] data, uint BlockOffset) {
            // 源程序 放入表头 + 最后4个地址 + 文件类型 + 文件名（1B*4=6c = 108）
            //Console.WriteLine(" ！开始获取文件在PDE中的偏移信息");
            //Console.WriteLine(" 获取偏移传入数据大小: " + data.Length);
            // 块大小
            int BlockSize = 128;
            // 块数量
            int BlockCount = data.Length / BlockSize;
            // 块数组
            byte[][] BlockArr = new byte[BlockCount][];
            // 循环分块
            for (int i = 0; i < BlockCount; i++) {
                int start = i * BlockSize;
                int length = Math.Min(BlockSize, data.Length - start);
                BlockArr[i] = new byte[length];
                Array.Copy(data, start, BlockArr[i], 0, length);
            }

            // 获取到的OffsetInfo数组
            List<HexOffsetInfo> OffsetInfoArr = [];

            // 循环获取偏移,大小,类型,文件名
            for (int bi = 0; bi < BlockCount; bi++) {
                byte[] Block = BlockArr[bi];
                //}

                //foreach (byte[] Block in BlockArr) {
                // 新Info
                HexOffsetInfo ThisInfo = new() {
                    Type = 0,
                    Name = "",
                    Offset = 0,
                    Size = 0,
                    OOffset = 0,
                };

                // 读取类型 1==文件，2==目录
                // 第一个00字节的索引  
                int NonZeroIndex = -1;
                // 获取 第1个非0，文件名，类型
                for (int i = 0; i < Block.Length; i++) {
                    // 获取类型
                    if (i == 0) {
                        // 获取到类型
                        ThisInfo.Type = Block[i];
                        // 验证类型
                        if (ThisInfo.Type != 1 && ThisInfo.Type != 2) {
                            //Console.WriteLine(" ！类型不为1或2退出循环:" + ThisInfo.Type);
                            break;
                        }
                        //Console.WriteLine(" ！类型正确？继续！！！");
                        continue;
                    } else {
                        // 获取名称
                        // 从汇编中找到的验证逻辑
                        int sub = Block[i] - 0x41;
                        if (sub <= 25) {
                            //Console.WriteLine(" ！<= 25 :" + sub);
                            sub += 32;
                        }
                        // 找到第1个非0字节
                        if (sub <= 0) {
                            //Console.WriteLine("<= 0 :" + sub);
                            NonZeroIndex = i - 1;
                            break;
                        }
                    }
                }

                // 读取文件名
                // 汇编中是读了108位 1B*4=6c = 108
                if (NonZeroIndex != -1) {
                    // 复制名称
                    ThisInfo.Name = Encoding.ASCII.GetString(Block, 1, NonZeroIndex);
                    // 判断名称是否合法
                    if (!NameValidator.Check(ThisInfo.Type, ThisInfo.Name))
                        break;
                } else {
                    // 获取文件名出错
                    break;
                }

                // 读取大小
                byte[] ThisSize = new byte[4];
                Array.Copy(Block, Block.Length - 4, ThisSize, 0, 4);
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(ThisSize);
                }
                ThisInfo.Size = BitConverter.ToUInt32(ThisSize, 0);

                // 读取原始偏移值 
                byte[] ThisOffset = new byte[4];
                Array.Copy(Block, Block.Length - 8, ThisOffset, 0, 4);
                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(ThisOffset);
                }
                // 保存原始偏移值
                ThisInfo.OOffset = BitConverter.ToUInt32(ThisOffset, 0);

                // 计算实际偏移值
                ThisInfo.Offset = ((ThisInfo.OOffset >> 10) + ThisInfo.OOffset + 1) << 12;

                // 如果 ThisInfo.Offset 越界，则退出循环
                if (ThisInfo.Offset >= PDESIZE) {
                    Console.Error.WriteLine(" ！ThisInfo.Offset 越界退出循环");
                    // 记录未知文件或目录的偏移到日志
                    if (GVar.NeedOffsetLog) {
                        OffsetLog.Rec(BlockOffset, ThisInfo.Offset, ThisInfo.OOffset, ThisInfo.Size, "未知文件名", ThisInfo.Type, THISPDENAME.Name + "/");
                    }
                    continue;
                }

                // 读取标识
                uint TagOffSet = (uint)(BlockOffset + ((bi + 1) * 0x80) - 0xC);
                GetOffsetStr ThisTag = GetByteOfPde(TagOffSet, 4);
                // 打印 ThisTag.Byte
                //Console.WriteLine(" ！ThisTag.Byte: " + BitConverter.ToString(ThisTag.Byte));
                GVar.TagsHash.Add(BitConverter.ToString(ThisTag.Byte).Replace("-", ""));
                //Console.WriteLine(" ！TagsHash长度: " + GVar.TagsHash.Count);

                // 添加到OffsetInfoArr
                OffsetInfoArr.Add(ThisInfo);
            }

            //Console.WriteLine("√ 获取文件在PDE中的偏移信息完成");
            return OffsetInfoArr;
        }

        /// <summary>
        /// 从PDE文件中获取指定块数据
        /// </summary>
        /// <param name="Start">块在PDE文件中的起始偏移</param>
        /// <param name="Size">块大小</param>
        /// <returns>块数据</returns>
        public static GetOffsetStr GetByteOfPde(uint Start, uint Size) {
            // 读取PDE文件  
            using FileStream PDEFILE_FS = new(THISPDENAME.FullName, FileMode.Open);

            // 设置读取起点
            PDEFILE_FS.Position = Start;

            // 创建BinaryReader对象  
            using BinaryReader PDEREADER = new(PDEFILE_FS);

            // 初始缓冲区大小
            // 初始化 BufferRes.Byte
            try {
                GetOffsetStr BufferRes = new() { Size = Size, Byte = new byte[Size] };

                // 用于保存实际数据的有效长度  
                int validDataLength = 0;
                // PDEREADER 流读取器  
                int bytesRead = PDEREADER.Read(BufferRes.Byte, validDataLength, BufferRes.Byte.Length - validDataLength);
                // 更新有效数据的长度  
                validDataLength += bytesRead;

                // 如果读取的字节数小于缓冲区大小，说明可能已到达文件末尾  
                if (bytesRead < BufferRes.Byte.Length) {
                    Console.WriteLine(" ！DEBUG: 读取的字节数小于缓冲区大小，说明可能已到达文件末尾");
                    // 创建一个新的、更小的缓冲区来存储剩余的数据  
                    byte[] newBuffer = new byte[validDataLength];
                    Array.Copy(BufferRes.Byte, newBuffer, validDataLength);
                    BufferRes.Byte = newBuffer;
                    BufferRes.Size = (uint)newBuffer.Length;
                }

                // 返回读取的数据  
                return BufferRes;
            } catch (Exception e) {
                Console.WriteLine(" ！读取PDE文件失败: " + e.Message);
                // 失败返回空GetByteOfPde
                return new GetOffsetStr() { Size = 0, Byte = [] };
            }
        }

        /// <summary>
        /// 解密文件 或! 数据块
        /// </summary>
        /// <param name="OffsetArr">文件或数据块的偏移信息数组</param>
        /// <param name="FixSize">是否修正大小</param>
        /// <returns>解密后的数据</returns>
        public static byte[] DeFileOrBlock(byte[] OffsetArr, bool FixSize) {
            // 当前临时解密字节数组
            byte[] TempDEArr;
            if (FixSize) {
                // todo: +0x4目的是为了不越位
                TempDEArr = new byte[OffsetArr.Length + 0x4];
            } else {
                TempDEArr = new byte[OffsetArr.Length];
            }
            // Key长度
            int KeyLenght = PDEKEY.Length - 1;
            // 密钥索引
            int KeyIndex = 0;
            // 循环处理每个字节
            for (int i = 0; i < OffsetArr.Length; i++) {
                // 异或操作
                int XorVal = PDEKEY[KeyIndex] ^ OffsetArr[i];
                TempDEArr[i] = (byte)XorVal;
                //Console.WriteLine(" ！i: " + i + " ki:" + KeyIndex);
                // 如果KeyIndex越界，则重新开始
                if (KeyIndex >= KeyLenght) {
                    //Console.WriteLine(" ！KeyIndex越界 归零");
                    KeyIndex = 0;
                } else {
                    KeyIndex++;
                }
            }

            // 完成解密数据块
            return TempDEArr;
        }
    }
}
