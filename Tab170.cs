using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using static Unpde.DataType;
using static Unpde.PdeTool;

namespace Unpde {
    /// <summary>
    /// 170类
    /// </summary>
    internal class Tab170 {

        /// <summary>
        /// 170表
        /// </summary>
        private static List<Table170> OFFSET170 = [];
        private static List<Table170LogVer> TEMP170LOG = [];
        // TODO:测试用待删
        public static List<uint> TEMP170 = [];

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init() {
            // 计算170表
            Calc170();

            // 保存170表
            Save170();

            // 循环解密
            Run();
        }

        /// <summary>
        /// 解密170表
        /// </summary>
        private static void Run() {
            Console.WriteLine(" ！开始循环解密170");
            // 循环解密 OFFSET170
            // 目的是将170表？写入到调试用PDE
            foreach (Table170 T170 in OFFSET170) {
                DirStr Dir170 = new() { UpDir = ThisPdeName.Name + "/", NowDir = ThisPdeName.Name + "/" };
                // True == 170表， False == 非170表
                Unpack.Try(T170.Offset, T170.Size, Dir170, true);
            }
            Console.WriteLine("√ 完成循环解密170");
        }

        /// <summary>
        /// 计算170列表
        /// </summary>
        private static void Calc170() {
            Console.WriteLine(" ！开始计算170表");
            // 待解数据表总量170 - 1 (汇编删除了最后一个)
            uint TableCount = 0xA9;
            // 基址
            uint EDI = 0x0;

            // 使用数组来存储计算结果
            List<Table170> Offset170Arr = [];
            // 日志版本
            List<Table170LogVer> Temp170LogArr = [];
            // TODO: 测试用待删
            List<uint> Temp170Arr = [];

            // 计算前170个数据块的地址
            while (Offset170Arr.Count < TableCount) {
                // 计算逻辑
                uint EAX = EDI;
                EAX = (EAX >> 0x0A) + EDI;
                EAX <<= 0x0C;

                // 将EAX的值存入Offset170Arr
                Offset170Arr.Add(new Table170 {
                    Size = 4096,
                    Offset = EAX
                });

                // 日志版本
                Temp170LogArr.Add(new Table170LogVer {
                    Size = "1000",
                    Offset = EAX.ToString("X")
                });

                // TODO:测试用 待删
                // 测试用
                Temp170Arr.Add((uint)EAX);

                // 基址+400h
                EDI += 0x400;
            }

            // 返回结果
            OFFSET170 = Offset170Arr;
            // 日志版本
            TEMP170LOG = Temp170LogArr;
            // TODO: 测试用待删
            TEMP170 = Temp170Arr;

            Console.WriteLine("√ 170表计算完成");
        }

        /// <summary>
        /// 保存170表
        /// </summary>
        static void Save170() {
            try {
                string JsonFilePath = Path.Combine(ThisPdeName.Name, "170.json");
                // 使用  Newtonsoft.Json 将 TEMP170LOG 转换成 json 格式，保存到 Unpde/170.json 文件中
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(TEMP170LOG);
                File.WriteAllText(JsonFilePath, json);
            } catch (Exception ex) {
                Console.WriteLine("保存170表失败！" + ex.Message);
            }
        }
    }
}
