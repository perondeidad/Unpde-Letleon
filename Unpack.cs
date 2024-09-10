using static Unpde.DataType;
using static Unpde.FinalUnpack;
using static Unpde.PdeTool;
using static Program;

namespace Unpde {
    /// <summary>
    /// 尝试解包类
    /// </summary>
    internal class Unpack {

        // 暂时不参与二次解密的文件
        //private static readonly string[] PassArr = [".fsb", ".swf", ".ttf", "version", "language"];

        /// <summary>
        /// 尝试解密
        /// </summary>
        /// <param name="Offset">数据块在PDE文件中的偏移值</param>
        /// <param name="Size">数据块大小</param>
        /// <param name="Dir">目录</param>
        /// <param name="Is170">是否为170表数据</param>
        public static void Try(uint Offset, uint Size, DirStr Dir, bool Is170) {
            Console.WriteLine(" ！正在尝试解密: " + Dir.NowDir);

            // 定义变量
            GetOffsetStr TryByte;
            byte[] DeTryByte;
            try {
                // 读取数据
                TryByte = GetByteOfPde(Offset, Size);
                // 校验数据
                if (TryByte.Size != Size)
                    return;
                // 解密数据块 -> 同时生成一个供调试时使用的PDE文件
                DeTryByte = DeFileOrBlock(TryByte.Byte, false);
            } catch (Exception e) {
                Console.WriteLine(" ！读取数据失败: " + e.Message);
                return;
            }

            //保存数据到DebugPde，调试时使用
            if (GVar.NeedDebugPde) {
                DebugPde.Rec(DeTryByte, Offset, Size, ThisPdeName.Name);
            }

            // 非170表数据
            if (!Is170) {
                // 获取文件或文件夹偏移信息
                List<HexOffsetInfo> DeTryByteArr = GetOffsetInfo(DeTryByte, Offset);
                // 读取并保存文件到硬盘
                Save(DeTryByteArr, Dir, Offset);
            }
        }

        /// <summary>
        /// 创建目录或文件
        /// </summary>
        /// <param name="DirOrFileArr">文件或目录数组</param>
        /// <param name="Dir">目录</param>
        /// <param name="BlockOffset">数据块在PDE文件中的偏移值</param>
        static void Save(List<HexOffsetInfo> DirOrFileArr, DirStr Dir, uint BlockOffset) {
            // 遍历目录或文件
            foreach (HexOffsetInfo DirOrFile in DirOrFileArr) {
                //文件
                if (DirOrFile.Type == 1) {
                    // 记录文件偏移信息
                    if (GVar.NeedOffsetLog) {
                        OffsetLog.Rec(BlockOffset, DirOrFile.Offset, DirOrFile.OOffset, DirOrFile.Size, DirOrFile.Name ?? "未知文件名", DirOrFile.Type, Dir.NowDir);
                    }
                    //获取指定偏移的字节数据
                    GetOffsetStr TempFileByte = GetByteOfPde(DirOrFile.Offset, DirOrFile.Size);
                    // 校验数据
                    if (TempFileByte.Size != DirOrFile.Size)
                        break;
                    //解密数据
                    byte[] DeTempFileByte = DeFileOrBlock(TempFileByte.Byte, true);
                    //判断是否是空文件
                    if (DeTempFileByte.Length == 0 || DirOrFile.Name == "" || DirOrFile.Name == null)
                        break;

                    //保存数据到DebugPde，调试时使用
                    if (GVar.NeedDebugPde) {
                        DebugPde.Rec(DeTempFileByte, DirOrFile.Offset, DirOrFile.Size, ThisPdeName.Name);
                    }


                    // 二次解密
                    byte[] FinalByte;
                    // 修正名称
                    string FixName;

                    bool HasCache = DirOrFile.Name.Contains(".cache");

                    // 判断文件名是否包含.cache文件,与是否需要二次解密
                    // 从而判断是否需要参与二次解密
                    if (GVar.NeedSecondaryDecryption & HasCache) { // 需要二次解密
                        try {
                            // 二次解密
                            //FinalByte = []; // 测试用
                            FinalByte = FinalDecrypt(DeTempFileByte, DirOrFile.Name);
                        } catch (Exception e) {
                            // 二次解密失败
                            FinalByte = [];
                            //Console.WriteLine(" ！二次解密失败: " + e.Message);
                        }
                    } else { // 不需要二次解密
                        Console.WriteLine(" ！跳过不参与二次解密文件: " + DirOrFile.Name);
                        FinalByte = [];
                    }

                    // 二次解密似乎是因为有些文件会有多个Size导致的！
                    // 二次解密完成
                    if (FinalByte.Length > 0) {
                        // 去除FixName中的.cache
                        FixName = DirOrFile.Name.Replace(".cache", "");
                        string SavePath = Dir.NowDir + FixName;
                        //保存文件,如果不存在则保存，存在则跳过
                        if (!File.Exists(SavePath)) {
                            // 判断 SavePath 的目录是否存在
                            if (!Directory.Exists(Dir.NowDir)) {
                                Directory.CreateDirectory(Dir.NowDir);
                            }
                            // 保存文件
                            File.WriteAllBytes(SavePath, FinalByte);
                        } else {
                            //Console.WriteLine("二次解密成功->文件已存在:" + FixName);
                        }
                    } else {
                        //二次解密失败，保存初次解密数据
                        // 带.cache的原始名称
                        FixName = DirOrFile.Name;
                        string SavePath = Dir.NowDir + FixName;
                        //保存文件,如果不存在则保存，存在则跳过
                        if (!File.Exists(SavePath)) {
                            // 判断 SavePath 的目录是否存在
                            if (!Directory.Exists(Dir.NowDir)) {
                                Directory.CreateDirectory(Dir.NowDir);
                            }
                            // 保存文件
                            File.WriteAllBytes(SavePath, DeTempFileByte);
                        } else {
                            //Console.WriteLine("二次解密失败->文件已存在:" + FixName);
                        }
                    }
                } else if (DirOrFile.Type == 2) {// 目录
                    // 记录目录偏移信息
                    if (GVar.NeedOffsetLog) {
                        OffsetLog.Rec(BlockOffset, DirOrFile.Offset, DirOrFile.OOffset, DirOrFile.Size, DirOrFile.Name ?? "未知目录名", DirOrFile.Type, Dir.NowDir);
                    }

                    // 这里仅获取目录下的文件，不创建目录!
                    // 拼接新目录路径
                    DirStr NewDir = new() { UpDir = Dir.NowDir, NowDir = Dir.NowDir + DirOrFile.Name + "/" };
                    // 创建目录
                    Directory.CreateDirectory(NewDir.NowDir);
                    // 递归解密
                    Try(DirOrFile.Offset, DirOrFile.Size, NewDir, false);
                } else {
                    Console.WriteLine(" ！其他类型: " + DirOrFile.Name);
                }
            }
        }
    }
}
