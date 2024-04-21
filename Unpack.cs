using System.Drawing;
using static Unpde.DataType;
using static Unpde.FinalUnpack;
using static Unpde.PdeTool;

namespace Unpde {
    /// <summary>
    /// 尝试解包类
    /// </summary>
    internal class Unpack {

        // 暂时不参与二次解密的文件
        private static readonly string[] PassArr = [".fsb", ".swf", ".ttf", "version", "language"];

        /// <summary>
        /// 尝试解密
        /// </summary>
        public static void Try(uint Offset, uint Size, DirStr Dir, bool Is170) {
            // 定义变量
            GetOffsetStr TryByte;
            byte[] DeTryByte;
            try {
                // 读取数据
                TryByte = GetByteOfPde(Offset, Size);
                // 校验数据
                if (TryByte.Size != Size) return;
                // 解密数据块 -> 同时生成一个供调试时使用的PDE文件
                DeTryByte = DeFileOrBlock(TryByte.Byte);
            } catch (Exception e) {
                Console.WriteLine(" ！读取数据失败: " + e.Message);
                return;
            }

            //保存数据到DebugPde，调试时使用
            //DebugPde.Rec(DeTryByte, Offset, Size, ThisPde.Name);

            // 非170表数据
            if (!Is170) {
                // 获取文件或文件夹偏移信息
                List<HexOffsetInfo> DeTryByteArr = GetOffsetInfo(DeTryByte);
                // 读取并保存文件到硬盘
                Save(DeTryByteArr, Dir);
            }
        }

        /// <summary>
        /// 创建目录或文件
        /// </summary>
        static void Save(List<HexOffsetInfo> DirOrFileArr, DirStr Dir) {
            // 遍历目录或文件
            foreach (HexOffsetInfo DirOrFile in DirOrFileArr) {
                //文件
                if (DirOrFile.Type == 1) {
                    // 记录文件偏移信息
                    OffsetLog.Rec(DirOrFile.Offset, DirOrFile.Size, DirOrFile.Name ?? "未知文件名", DirOrFile.Type, Dir.NowDir);
                    //获取指定偏移的字节数据
                    GetOffsetStr TempFileByte = GetByteOfPde(DirOrFile.Offset, DirOrFile.Size);
                    // 校验数据
                    if (TempFileByte.Size != DirOrFile.Size) break;
                    //解密数据
                    byte[] DeTempFileByte = DeFileOrBlock(TempFileByte.Byte);
                    //判断是否是空文件
                    if (DeTempFileByte.Length == 0 || DirOrFile.Name == "" || DirOrFile.Name == null) break;

                    //保存数据到DebugPde，调试时使用
                    //DebugPde.Rec(DeTempFileByte, DirOrFile.Offset, DirOrFile.Size, ThisPde.Name);

                    // 二次解密
                    byte[] FinalByte;
                    // 修正名称
                    string FixName;
                    // 跳过不参与二次解密的文件
                    if (PassArr.Any(DirOrFile.Name.ToLower().Contains)) {
                        Console.WriteLine(" ！跳过不参与二次解密文件: " + DirOrFile.Name);
                        FinalByte = [];
                    } else {
                        try {
                            // 二次解密
                            FinalByte = FinalDecrypt(DeTempFileByte, DirOrFile.Name);
                        } catch (Exception e) {
                            // 二次解密失败
                            FinalByte = [];
                            //Console.WriteLine(" ！二次解密失败: " + e.Message);
                        }
                    }

                    // 二次解密似乎是因为有些文件会有多个Size导致的！
                    // 二次解密完成
                    if (FinalByte.Length > 0) {
                        // 去除FixName中的.cache
                        FixName = DirOrFile.Name.Replace(".cache", "");
                        string SavePath = Dir.NowDir + FixName;
                        //保存文件,如果不存在则保存，存在则跳过
                        if (!File.Exists(SavePath)) {
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
                            File.WriteAllBytes(SavePath, DeTempFileByte);
                        } else {
                            //Console.WriteLine("二次解密失败->文件已存在:" + FixName);
                        }
                    }
                } else if (DirOrFile.Type == 2) {// 目录
                    // 记录目录偏移信息
                    OffsetLog.Rec(DirOrFile.Offset, DirOrFile.Size, DirOrFile.Name ?? "未知目录名", DirOrFile.Type, Dir.NowDir);

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
