using static Unpde.PdeTool;

namespace Unpde {
    public static class DebugPde {
        /// <summary>
        /// 调试用PDE
        /// </summary>
        static byte[] DebugFile = [];

        /// <summary>
        /// 当前PDE名称
        /// </summary>
        private static string PdeName = "";

        /// <summary>
        /// 记录数据到DebugPde
        /// </summary>
        public static void Rec(byte[] DeTryByte, uint Offset, uint Size, string ThisPdeName) {
            // 初始化DebugPde
            if (DebugFile.Length == 0 | PdeName != ThisPdeName) {
                // 初始化DebugPde
                DebugFile = new byte[GetPdeSize];
                // 保存PDE名称
                PdeName = ThisPdeName;
            }

            // 保存数据到DebugPde
            try {
                Array.Copy(DeTryByte, 0, DebugFile, Offset, Size);
            } catch {
                Console.WriteLine(" ！数据越界");
            }
        }

        /// <summary>
        /// 保存DebugPde
        /// </summary>
        public static void Save() {
            Console.WriteLine(" ！正在保存 DEBUGPDE " + ThisPde.Name + ".hex...");
            string filePath = Path.Combine(ThisPde.Name, ThisPde.Name + ".hex");
            if (!File.Exists(filePath)) {
                // 创建文件
                using FileStream fs = new(filePath, FileMode.Create);
                fs.Write(DebugFile, 0, DebugFile.Length);
                Console.WriteLine("√ " + ThisPde.Name + ".hex 已保存.");
            } else {
                Console.WriteLine("! " + ThisPde.Name + ".hex 已存在.");
            }
        }
    }
}
