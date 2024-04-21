using Newtonsoft.Json;
using System.Diagnostics;
using static Unpde.DataType;

namespace Unpde {
    /// <summary>
    /// 测试类
    /// </summary>
    internal class Test {

        // 测试版二次解密方法
        // 从汇编0x4CFA90处还原
        public static void TestDecryptFunc() {
            // 文件名
            string ThisFileName;
            // 待解密数据
            byte[] EncryptedData;
            // 解密后数据
            byte[] DecryptedData;
            // 读取字节规则
            uint[] ByteLimt = [4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0];

            // 寄存器
            // 待解开始地址
            uint EAX = 0;
            uint EBX = 1;
            uint ECX;
            // 退出条件
            uint EDX;
            // 下一个数据写入的地址
            uint EBP = 0;
            // 待解地址
            uint ESI = EAX;
            uint EDI;
            // 写入地址
            uint ESP10;
            // 解码后数据大小
            uint ESP58;

            // 读取 Unpde/目录下的初次解密出来的文件来测试
            string filePath = @"Unpde\27B85000_7A739_lb_bg_manage01.dds.cache";
            // 这个文件会出错
            //Unpde\mesh\prop\weapon\bazookas\107AB000_AE848_bazookas_d.dds.cache

            string fileNamec = Path.GetFileName(filePath);
            ThisFileName = fileNamec.Replace(".cache", "");
            byte[] BinHex = File.ReadAllBytes(filePath);

            // 获取解密后数据长度
            ESP58 = (uint)BitConverter.ToInt32(BinHex, 0x1D);
            // 初始化EncryptedData
            EncryptedData = new byte[BinHex.Length - 0x21];
            // 从 0x21 开始到 结束 复制到 EncryptedData
            Array.Copy(BinHex, 0x21, EncryptedData, 0, EncryptedData.Length);
            //初始化解密后数据
            DecryptedData = new byte[Convert.ToInt32(ESP58)];

            // 逻辑开始
            EDI = ESP58 - 1;
            ESP10 = EDI;
            EAX = EBX;

            Console.WriteLine(" ！正在二次解密...");

            // 循环中要使用的本地函数
            void Fun_4CFB99() {
                EDI = EBP;
                EDI -= EDX;
                EBX = 0;
                EDX = EBP;
                EDI -= EBP;
                do {
                    EAX = BitConverter.ToUInt32(DecryptedData, (int)(EDI + EDX));
                    Array.Copy(BitConverter.GetBytes(EAX), 0, DecryptedData, (int)EDX, 4);
                    EBX += 3;
                    EDX += 3;
                } while (EBX < ECX);
                EAX = ESP58;
                EDI = ESP10;
                EBP += ECX;
                EBX = 1;
            }

            while (true) {
                if (EAX == EBX) {
                    EAX = BitConverter.ToUInt32(EncryptedData, (int)ESI);
                    ESI += 4;
                }
                ECX = BitConverter.ToUInt32(EncryptedData, (int)ESI);
                if (((byte)EBX & (byte)EAX) != 0) {
                    EAX >>= 1;
                    ESP58 = EAX;
                    if (((byte)ECX & 0x3) == 0) {
                        ECX >>= 2;
                        ECX &= 0x3F;
                        EDX = ECX;
                        ECX = 3;
                        ESI += EBX;
                        Fun_4CFB99();
                    } else {
                        if (((byte)ECX & 0x2) == 0) {
                            ECX >>= 2;
                            ECX &= 0x3FFF;
                            EDX = ECX;
                            ECX = 3;
                            ESI += 2;
                            Fun_4CFB99();
                        } else {
                            EDX = ECX;
                            if (((byte)EBX & (byte)ECX) == 0) {
                                ECX >>= 2;
                                EDX >>= 6;
                                ECX &= 0xF;
                                EDX &= 0x3FF;
                                ECX += 3;
                                ESI += 2;
                                Fun_4CFB99();
                            } else {
                                EDX &= 0x7F;
                                if ((byte)EDX > 3) {
                                    EDX = ECX;
                                    ECX >>= 2;
                                    EDX >>= 7;
                                    ECX &= 0x1F;
                                    EDX &= 0x1FFFF;
                                    ECX += 2;
                                    ESI += 3;
                                    Fun_4CFB99();
                                } else {
                                    EDX = ECX;
                                    ECX >>= 7;
                                    ECX &= 0xFF;
                                    EDX >>= 15;
                                    ECX += 3;
                                    ESI += 4;
                                    Fun_4CFB99();
                                }
                            }
                        }
                    }
                } else {
                    EDX = EDI - 0xA;
                    // 退出条件
                    if (EBP > EDX) {
                        break;
                    } else {
                        byte[] ECXBytes = BitConverter.GetBytes(ECX);
                        Array.Copy(ECXBytes, 0, DecryptedData, EBP, ECXBytes.Length);
                        ECX = EAX;
                        ECX &= 0xF;
                        //ByteLimt -> [4, 0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0]");
                        ECX = ByteLimt[ECX];
                        EBP += ECX;
                        ESI += ECX;
                        EAX >>= (int)ECX;
                    }
                }
            }

            if (EBP < EDI) {
                do {
                    if (EAX == EBX) {
                        ESI += 4;
                        EAX = 0x80000000;
                    } else {
                        byte DL = EncryptedData[ESI];
                        DecryptedData[EBP] = DL;
                        EBP += EBX;
                        ESI += EBX;
                        EAX >>= 1;
                    }
                } while (EBP < EDI);
            }

            // 删除 DecryptedData 前8个字节
            Array.Copy(DecryptedData, 8, DecryptedData, 0, DecryptedData.Length - 8);
            Array.Resize(ref DecryptedData, DecryptedData.Length - 8);

            // 写入 DecryptedData 到 Unpde/ 文件夹下
            File.WriteAllBytes("Unpde/" + ThisFileName, DecryptedData);
            Console.WriteLine(ThisFileName + "完成！");
        }

        /// <summary>
        /// 计算从PDE中获取的原始偏移值到实际偏移值
        /// </summary>
        public static void CalcOffset(string HexOffset) {
            Console.WriteLine(" ！计算偏移值: " + HexOffset);
            // 将16进制字符串转换为uint
            uint OffsetUint = Convert.ToUInt32(HexOffset, 16);
            Console.WriteLine(" ！10进制: " + OffsetUint);
            uint OffsetPde = ((OffsetUint >> 10) + OffsetUint + 1) << 12;
            Console.WriteLine(" ！结果: " + OffsetPde.ToString("X"));
        }

        /// <summary>
        /// 测试Json偏移
        /// </summary>
        public static void TestJson() {
            string TestJson = File.ReadAllText("testoffset.json");
            // 使用 Newtonsoft.Json 解析Json
            IDictionary<string, OffsetJsonStr>? TestJsonArr = JsonConvert.DeserializeObject<IDictionary<string, OffsetJsonStr>>(TestJson);

            // 解析失败
            if (TestJsonArr == null) {
                Console.WriteLine(" ！Json解析失败");
                return;
            }

            //遍历Json数组
            foreach (KeyValuePair<string, OffsetJsonStr> Jsoni in TestJsonArr) {
                //Console.WriteLine(" ！Json: " + Jsoni.Key + " _ " + Jsoni.Value.Size);
                // 将 Jsoni.Key 转换位 uint 类型
                //uint OffsetUint = Convert.ToUInt32(Jsoni.Key, 16);
                for (int i = 0; i < Jsoni.Value.Size.Count; i++) {
                    DirStr NowDir = new() { UpDir = "Unpde/", NowDir = "Unpde/" };

                    if (Tab170.TEMP170.Contains(Jsoni.Value.Offset)) {
                        Unpack.Try(Jsoni.Value.Offset, Jsoni.Value.Size[i], NowDir, true);
                    } else {
                        Unpack.Try(Jsoni.Value.Offset, Jsoni.Value.Size[i], NowDir, false);
                    }
                }
            }
            Console.WriteLine(" ！Json解析完成");
        }
    }
}
