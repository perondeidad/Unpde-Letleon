using static Unpde.DataType;

namespace Unpde {
    internal class GetPDEList {
        /// <summary>
        /// 获取当前目录下的所有.pde文件名
        /// </summary>
        public static List<PdeNames> Get() {
            List<PdeNames> pdeList = [];
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.pde");
            // 提取文件名到列表中
            foreach (string file in files) {
                PdeNames ThisPde = new() {
                    // 去掉扩展名
                    Name = Path.GetFileNameWithoutExtension(file),
                    // 完整名称
                    FullName = Path.GetFileName(file)
                };
                Console.WriteLine(" ！找到.pde文件：" + ThisPde.FullName);
                pdeList.Add(ThisPde);
            }

            // 如果没有找到.pde文件,则退出程序
            if (pdeList.Count == 0) {
                Console.WriteLine(" ！目录下没有找到 *.pde文件！\n 按任意键退出程序");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            // 返回.pde文件名列表
            return pdeList;
        }
    }
}
