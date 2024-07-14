namespace Unpde {
    /// <summary>
    /// PDE Key类.
    /// </summary>
    internal class PdeKey {

        // 版本 1.0.1.9920 KEY位置(每个位置的值都一样)
        // 101E1000 10263000 2708F000 275BD000 27A3E000

        // 版本 1.0.6.7960 KEY位置(每个位置的值都一样)
        // CE25000 1C9CF000 29F40000 ...还有21个

        /// <summary>
        /// PDEKEY
        /// </summary>
        /// <returns>PDEKEY</returns>
        public static byte[] PDEKEY() {
            // PDEKEY
            byte[] KeyByte = new byte[0x1000];

            // 逻辑从 汇编 0x00A608E0 处获得
            uint EAX = 0x42574954;
            for (int i = 0; i < 0x1000; i++) {
                EAX *= 0x7FCF;
                uint ECX = EAX;
                ECX >>= 0x18;
                uint EDX = EAX;
                EDX >>= 0x10;
                byte CL = (byte)((byte)ECX ^ (byte)EDX);
                EDX = EAX;
                EDX >>= 0x8;
                CL = (byte)(CL ^ (byte)EDX);
                CL = (byte)(CL ^ (byte)EAX);
                KeyByte[i] = CL;
            }
            Console.WriteLine(" √PDEKEY计算完成");
            return KeyByte;
        }
    }
}
