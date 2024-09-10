namespace Unpde {
    /// <summary>
    /// 最终解密类
    /// </summary>
    internal class FinalUnpack {
        /// <summary>
        /// 二次解密方法
        /// 从汇编0x4CFA90处还原
        /// </summary>
        /// <param name="DeTempFileByte">需要二次解密的初次解密后的字节数组</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] FinalDecrypt2(byte[] DeTempFileByte) {
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

            // 解码后大小标识
            byte SizeFlag = DeTempFileByte[0x18];

            // 根据标识设置偏移值
            byte[] EncryptedData;
            (ESP58, EncryptedData) = SizeFlag switch {
                0x6F => (BitConverter.ToUInt32(DeTempFileByte, 0x1D), DeTempFileByte[0x21..]),
                0x6D => (DeTempFileByte[0x1A], DeTempFileByte[0x1B..]),
                _ => throw new Exception("未知的解码后大小标识")
            };

            // 初始化解密后数据
            DecryptedData = new byte[ESP58];

            // 逻辑开始
            EDI = ESP58;
            ESP10 = EDI;
            EAX = EBX;

            try {
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

                    // 优化越界检查
                    if (ESI + 4 > EncryptedData.Length) {
                        Console.WriteLine("解码完成，返回解码数据");
                        break;
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
                                EDX = ECX;// ???
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
                                        EDX = ECX;// ???
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

                // 循环结束后，将剩余的字节写入 DecryptedData
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

                // 优化删除前8个字节的操作
                return DecryptedData[8..];
            } catch (Exception ex) {
                Console.WriteLine($"异常：{ex.Message}");
                // 异常也返回解密后数据
                return DecryptedData[8..];
            }
        }

        /// <summary>
        /// 最终解密判断
        /// </summary>
        /// <param name="DeTempFileByte">初次解密后的字节数组</param>
        /// <param name="FileName">文件名</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] FinalDecrypt(byte[] DeTempFileByte, string FileName) {
            Console.WriteLine($"正在二次解密的文件名: {FileName}");
            if (FileName == "ak47_sight.dds.cache") {
                Console.WriteLine("🐞！ak47_sight.dds.cache");
            }

            byte REG_AL = DeTempFileByte[0x18];
            Console.WriteLine($"REG_AL: 0x{REG_AL:X2}");

            return (REG_AL & 1) != 0
                ? FinalDecrypt2(DeTempFileByte)
                : DeTempFileByte.Length <= 0x29
                    ? throw new Exception("初次解密后的字节数组长度不足，无法删除前0x29个字节")
                    : DeTempFileByte[0x29..];
        }
    }
}