//////////////////////
// 由智谱清言GLM-4编写
//////////////////////
namespace Unpde {
    /// <summary>
    /// 调试PDE类
    /// </summary>
    public static class DebugPde {
        // 当前PDE名称
        private static string PdeName = "";
        // 调试文件路径
        private static string DebugFilePath = "";

        /// <summary>
        /// 记录调试数据
        /// </summary>
        /// <param name="data">初次解码数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="size">大小</param>
        /// <param name="ThisPdeName">当前PDE名称</param>
        public static void Rec(byte[] data, uint offset, uint size, string ThisPdeName) {
            // 如果PDE名称改变,则初始化
            if (PdeName != ThisPdeName) {
                // 记录当前PDE名称
                PdeName = ThisPdeName;
                // 调试文件路径为PDE名称加上.hex后缀
                DebugFilePath = Path.Combine(PdeName, PdeName + ".hex");
            }

            // 使用FileStream来写入数据
            try {
                using FileStream fs = new(DebugFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(data, 0, (int)size);
            } catch (Exception ex) {
                Console.WriteLine($"写入文件时出错: {ex.Message}");
            }
        }
    }
}