namespace Unpde {
    /// <summary>
    /// 偏移计算器
    /// </summary>
    internal class OffsetCalc {
        /// <summary>
        /// 原始偏移值转PDE实际偏移值
        /// </summary>
        /// <param name="OriginalOffset">原始偏移值</param>
        public static void OriginalToPde(uint OriginalOffset) {
            Console.WriteLine(" ！计算偏移值: " + OriginalOffset.ToString("X"));
            // 逻辑是从汇编中得到的
            uint OffsetPde = ((OriginalOffset >> 10) + OriginalOffset + 1) << 12;
            Console.WriteLine(" ！结果: " + OffsetPde.ToString("X"));

            PdeToOriginal(OffsetPde);
        }

        /// <summary>
        /// PDE实际偏移值转原始偏移值
        /// 该代码段由 Moonshot AI 开发的 Kimi 智能助手提供帮助
        /// </summary>
        /// <param name="PdeOffset">PDE实际偏移值</param>
        public static void PdeToOriginal(uint PdeOffset) {
            Console.WriteLine(" ！还原偏移值: " + PdeOffset.ToString("X"));
            // 结果
            uint originalOffset;

            // 原始偏移值 0xC90DA 转换成PDE实际偏移值 后得到 0xC93FF000
            // 测试后发现 PdeOffset 超过了 0xC93FF000 就会不正确，所以需要判断下
            // todo: 判断不对 script 的PDE原始偏移值是 F9670200，计算结果是267f8 ,应为267f9
            if (PdeOffset <= 0xC93FF000) {
                // 未超过 0xC93FF000 则 -1
                originalOffset = (PdeOffset >> 12) - (PdeOffset >> 22) - 1;
            } else {
                // 超过 0xC93FF000 则 不-1
                originalOffset = (PdeOffset >> 12) - (PdeOffset >> 22);
            }
            Console.WriteLine(" ！还原的原始偏移值: " + originalOffset.ToString("X"));
        }
    }
}
