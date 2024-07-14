using System.Buffers;
using static Unpde.DataType;
using static Unpde.PdeTool;

namespace Unpde {
    /// <summary>
    /// 查找未解码目录类
    /// </summary>
    internal class FindDir {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private const int BufferSize = 0x1000000;
        /// <summary>
        /// FindDirPDE文件地址
        /// </summary>
        private static string FindDirPDEPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThisPdeName.Name, "FindDirPDE.hex");
        /// <summary>
        /// 原始PDE文件地址
        /// </summary>
        private static string PDEPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThisPdeName.FullName);

        /// <summary>
        /// 记录偏移值覆盖信息
        /// </summary>
        /// <param name="Offset">偏移值</param>
        /// <param name="Size">大小</param>
        public static void Rec(uint Offset, uint Size) {
            // FindDirPDE 不存在则创建
            if (!File.Exists(FindDirPDEPath)) {
                Console.WriteLine(" √FindDirPDE文件不存在，正在创建...");
                using FileStream fs = File.Create(FindDirPDEPath);
                // 设置文件长度为PdeSize
                fs.SetLength(PdeSize);
            }

            // 将已覆盖区域标记为1
            using (FileStream fs = new(FindDirPDEPath, FileMode.Open, FileAccess.ReadWrite)) {
                fs.Seek(Offset, SeekOrigin.Begin);
                byte[] buffer = Enumerable.Repeat((byte)1, (int)Size).ToArray();
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// 查找未解码目录
        /// </summary>
        public static void Find() {
            Console.WriteLine(" ！开始查找未解码目录...");
            Console.WriteLine(" ！TagsHash长度:" + GVar.TagsHash.Count.ToString());
            try {
                using var fs = new FileStream(FindDirPDEPath, FileMode.Open, FileAccess.Read);
                // 读取FindDirPDE,查找未覆盖区域,找到文件或文件夹标记区域
                List<UnDecodedDir> undecodedBlocks = FindUndecodedBlocks(fs);
                fs.Close();

                // 存在目录或文件
                if (undecodedBlocks.Count > 0) {
                    Console.WriteLine(" ！本次总共找到 " + undecodedBlocks.Count + " 个未解码区域");
                    Console.WriteLine(" ！前往解包");
                    // 尝试解包
                    TryUnpack(undecodedBlocks);
                } else {
                    Console.WriteLine(" √未找到未解码目录");
                }
            } catch (Exception ex) {
                Console.WriteLine(" ！查找未解码目录失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 尝试解包
        /// </summary>
        /// <param name="unrecordedOffsets">未记录的目录偏移列表</param>
        private static void TryUnpack(List<UnDecodedDir> unrecordedOffsets) {
            foreach (UnDecodedDir unOffset in unrecordedOffsets) {
                uint nowOffset = (uint)unOffset.OffSet;
                uint NewSize = (uint)(unOffset.Size < 0x1000 ? unOffset.Size : 0x1000);

                // 创建目录
                var tryDir = new DirStr { UpDir = ThisPdeName.Name + "/Other/", NowDir = ThisPdeName.Name + "/Other/" + unOffset.OffSet + "/" };

                // 记录日志，一定要记录
                if (GVar.NeedOffsetLog) {
                    OffsetLog.Rec(nowOffset, nowOffset, nowOffset, NewSize, ThisPdeName.Name + "/Other/", 2, ThisPdeName.Name + "/Other/" + unOffset.OffSet + "/");
                }

                // 尝试解包
                Unpack.Try(nowOffset, NewSize, tryDir, false);
            }

            Console.WriteLine(" √此次解包完成,再次查找目录！");
            unrecordedOffsets.Clear();
            // 再次查找
            Find();
        }

        /// <summary>
        /// 查找未解码目录
        /// </summary>
        /// <param name="fs">FindDirPDE文件流</param>
        /// <returns>未解码目录列表</returns>
        private static List<UnDecodedDir> FindUndecodedBlocks(FileStream fs) {
            // 未解码目录列表
            var undecodedBlocks = new List<UnDecodedDir>();
            // Buffer
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try {
                int bytesRead;
                long currentOffset = 0;
                while ((bytesRead = fs.Read(buffer, 0, BufferSize)) > 0) {
                    var span = new ReadOnlySpan<byte>(buffer, 0, bytesRead);
                    int startIndex = 0;
                    while (startIndex < bytesRead) {
                        int nonZeroIndex = span[startIndex..].IndexOfAnyExcept(new byte[] { 0 });
                        if (nonZeroIndex == -1) {
                            break;
                        }
                        startIndex += nonZeroIndex;
                        int endIndex = startIndex + 1;
                        while (endIndex < bytesRead && span[endIndex] == 0) {
                            endIndex++;
                        }
                        long size = endIndex - startIndex;
                        // 0x1000 or 0x80
                        // 一般一个目录或目录块大小是0x1000，0x80 是最小单元，也就是只能存一个目录或文件
                        if (size >= 0x80) {
                            long offset = currentOffset + startIndex;
                            // 记录未解码区域
                            UnDecodedDir ThisUnDecodedDir = new() { OffSet = 0, Size = 0 };
                            // 读取原始PDE区域
                            using var blockStream = new FileStream(PDEPath, FileMode.Open, FileAccess.Read);
                            blockStream.Position = offset;
                            var blockData = new byte[size];
                            blockStream.Read(blockData, 0, (int)size);

                            var fIndex = FindHexPattern(blockData, GVar.TagsHash);
                            if (fIndex != -1) {
                                //Console.WriteLine(" 找到16进制字符串: " + fIndex.ToString("X"));
                                //Console.WriteLine(" 偏移地址: " + offset.ToString("X"));
                                //Console.WriteLine(" 大小: " + size.ToString("X"));

                                // 计算新偏移地址
                                uint NewOffset = (uint)(offset + (fIndex - 0x74));

                                // 记录偏移值覆盖信息
                                ThisUnDecodedDir.OffSet = NewOffset;
                                ThisUnDecodedDir.Size = size;
                            }

                            // 判断是否存在文件或文件夹
                            if (ThisUnDecodedDir.Size != 0) {
                                // 记录未解码区域
                                undecodedBlocks.Add(ThisUnDecodedDir);
                            }
                        }
                        startIndex = endIndex;
                    }
                    currentOffset += bytesRead;
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            // 返回
            return undecodedBlocks;
        }

        /// <summary>
        /// 查找文件夹或文件特殊标记是否存在
        /// </summary>
        /// <param name="byteData">原始字节数据</param>
        /// <param name="fdKey">文件夹或文件特殊标记</param>
        /// <returns>-1: 不存在, 其他: 偏移地址 </returns>
        private static int FindHexPattern(byte[] byteData, HashSet<string> fdKey) {
            foreach (var hexPattern in fdKey) {
                var patternBytes = StringToByteArray(hexPattern);
                var index = FindByteArray(byteData, patternBytes);
                if (index != -1) {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// 字符串转字节数组
        /// </summary>
        /// <param name="hex"> 16进制字符串 </param>
        /// <returns> 字节数组 </returns>
        private static byte[] StringToByteArray(string hex) {
            int length = hex.Length / 2;
            var bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// 查找字节数组
        /// </summary>
        /// <param name="data"> 原始字节数据 </param>
        /// <param name="pattern"> 查找字节数组 </param>
        /// <returns> -1: 不存在, 其他: 偏移地址 </returns>
        private static int FindByteArray(byte[] data, byte[] pattern) {
            int patternLength = pattern.Length;
            for (int i = 0; i <= data.Length - patternLength; i++) {
                bool match = true;
                for (int j = 0; j < patternLength; j++) {
                    if (data[i + j] != pattern[j]) {
                        match = false;
                        break;
                    }
                }
                if (match) {
                    return i;
                }
            }
            return -1;
        }
    }
}