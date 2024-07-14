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
            // TODO:这个函数不完善，存在越位问题！

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

            // 解码后大小标识
            byte SizeFlag = DeTempFileByte[0x18];
            //Console.WriteLine(" ！SizeFlag:" + SizeFlag.ToString("X"));

            // 根据标识设置偏移值
            switch (SizeFlag.ToString("X")) {
                case "6F":// 常规文件? 6F 标识
                          // 获取解密后数据长度
                    ESP58 = BitConverter.ToUInt32(DeTempFileByte, 0x1D);
                    // 初始化EncryptedData大小
                    EncryptedData = new byte[DeTempFileByte.Length - 0x21];
                    // 从 0x21 开始到 结束 复制到 EncryptedData
                    Array.Copy(DeTempFileByte, 0x21, EncryptedData, 0, EncryptedData.Length);
                    break;
                case "6D":// 非常规文件? 6D 标识
                          // 获取解密后数据长度
                    ESP58 = DeTempFileByte[0x1A];
                    // 初始化EncryptedData大小
                    EncryptedData = new byte[DeTempFileByte.Length - 0x1B];
                    // 从 0x1A 开始到 结束 复制到 EncryptedData
                    Array.Copy(DeTempFileByte, 0x1B, EncryptedData, 0, EncryptedData.Length);
                    break;
                default:
                    // 未知文件类型
                    throw new Exception(" ！解码后大小标识");
            }

            //初始化解密后数据
            DecryptedData = new byte[ESP58];

            // 逻辑开始
            EDI = ESP58 - 1;
            ESP10 = EDI;
            EAX = EBX;

            //Console.WriteLine(" ！正在二次解密...");

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

                    // TODO:会越界！实际上已经解码完成了！
                    //if (SizeFlag == 0x6D) {
                    //    if (EncryptedData.Length - (int)ESI >= 4) {
                    //        ECX = BitConverter.ToUInt32(EncryptedData, (int)ESI);
                    //    } else {
                    //        Console.WriteLine(" ！越界了，返回解码数据！！！！");
                    //        return DecryptedData;
                    //    }
                    //} else {
                    ECX = BitConverter.ToUInt32(EncryptedData, (int)ESI);
                    //}

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

                // 删除 DecryptedData 前8个字节
                Array.Copy(DecryptedData, 8, DecryptedData, 0, DecryptedData.Length - 8);
                Array.Resize(ref DecryptedData, DecryptedData.Length - 8);

                // 返回
                //Console.WriteLine("√二次解密完成！");
                return DecryptedData;
            } catch (Exception ex) {
                Console.WriteLine(" ！异常：" + ex.Message);
                return DecryptedData;
            }
        }

        /// <summary>
        /// 最终解密判断
        /// </summary>
        /// <param name="DeTempFileByte">初次解密后的字节数组</param>
        /// <param name="FileName">文件名</param>
        /// <returns>解密后的字节数组</returns>
        public static byte[] FinalDecrypt(byte[] DeTempFileByte, string FileName) {
            Console.WriteLine("正在二次解密的文件名: " + FileName);
            byte REG_AL = DeTempFileByte[0x18];
            Console.WriteLine("REG_AL: 0x" + REG_AL.ToString("X2"));
            if ((REG_AL & 1) != 0) {
                return FinalDecrypt2(DeTempFileByte);
            } else {
                if (DeTempFileByte.Length <= 0x29) {
                    throw new Exception("初次解密后的字节数组长度不足，无法删除前0x29个字节");
                }
                byte[] result = new byte[DeTempFileByte.Length - 0x29];
                Array.Copy(DeTempFileByte, 0x29, result, 0, result.Length);
                Console.WriteLine($"{FileName} 无需二次解密");
                return result;
            }
        }
    }
}