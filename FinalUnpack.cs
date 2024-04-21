namespace Unpde {
    internal class FinalUnpack {
        /// <summary>
        /// 二次解密方法
        /// 从汇编0x4CFA90处还原
        /// </summary>
        public static byte[] FinalDecrypt2(byte[] DeTempFileByte) {
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

            // 获取解密后数据长度
            ESP58 = BitConverter.ToUInt32(DeTempFileByte, 0x1D);

            // 初始化EncryptedData
            EncryptedData = new byte[DeTempFileByte.Length - 0x21];
            // 从 0x21 开始到 结束 复制到 EncryptedData
            Array.Copy(DeTempFileByte, 0x21, EncryptedData, 0, EncryptedData.Length);

            //初始化解密后数据
            DecryptedData = new byte[ESP58];

            // 逻辑开始
            EDI = ESP58 - 1;
            ESP10 = EDI;
            EAX = EBX;

            //Console.WriteLine(" ！正在二次解密...");

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

            // 返回
            //Console.WriteLine("√二次解密完成！");
            return DecryptedData;
        }

        /// <summary>
        /// 最终解密判断
        /// </summary>
        public static byte[] FinalDecrypt(byte[] DeTempFileByte, string FileName) {
            // 从汇编中获取的判断逻辑
            // 获取 DeTempFileByte 0x18处的一个byte
            byte REG_AL = DeTempFileByte[0x18];
            // 判断是否需要二次解密
            if ((REG_AL & 1) != 0) {
                // 需要二次解密
                return FinalDecrypt2(DeTempFileByte);
            } else {
                // 不需要二次解密
                // 删除 DeTempFileByte 前0x28个字节
                Array.Copy(DeTempFileByte, 0x29, DeTempFileByte, 0, DeTempFileByte.Length - 0x29);
                Array.Resize(ref DeTempFileByte, DeTempFileByte.Length - 29);
                Console.WriteLine(" √" + FileName + "无需二次解密");
                return DeTempFileByte;
            }
        }
    }
}