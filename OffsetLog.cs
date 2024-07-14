//////////////////////////////////
// Code by claude-3-opus-20240229
//////////////////////////////////
using static Unpde.PdeTool;

namespace Unpde {

    internal class NewOffsetType {
        public uint Offset { get; set; }
        public uint Size { get; set; }
        // 可以添加其他需要的属性或方法
    }

    /// <summary>
    /// 偏移值信息记录
    /// </summary>
    internal class OffsetLog {
        /// <summary>
        /// 目录结构偏移值信息
        /// </summary>
        private static Dictionary<string, object> DirStructureOffsets = [];

        /// <summary>
        /// 记录偏移值
        /// </summary>
        /// <param name="BOffset">数据块在PDE文件中的偏移值</param>
        /// <param name="Offset">真实偏移值</param>
        /// <param name="OOffset">原始偏移值</param>
        /// <param name="Size">大小</param>
        /// <param name="Name">文件或文件夹名称</param>
        /// <param name="Type">文件或文件夹类型</param>
        /// <param name="ParentPath">保存位置</param>
        public static void Rec(uint BOffset, uint Offset, uint OOffset, uint Size, string Name, uint Type, string ParentPath) {
            // 记录偏移值覆盖信息
            FindDir.Rec(Offset, Size);

            // 记录目录结构偏移值信息
            if (string.IsNullOrEmpty(ParentPath)) {
                if (Type == 1) {
                    ((List<object>)DirStructureOffsets["Files"]).Add(new {
                        Type,
                        Name,
                        BOffset = BOffset.ToString("X"),
                        OOffset = OOffset.ToString("X"),
                        Offset = Offset.ToString("X"),
                        Size = Size.ToString("X")
                    });
                } else {
                    DirStructureOffsets[Name] = new Dictionary<string, object> {
                        ["_info"] = new {
                            Type,
                            Name,
                            BOffset = BOffset.ToString("X"),
                            OOffset = OOffset.ToString("X"),
                            Offset = Offset.ToString("X"),
                            Size = Size.ToString("X")
                        },
                        ["Files"] = new List<object>()
                    };
                }
            } else {
                // 加1是为了包含"/"字符  
                int startIndex = ThisPdeName.Name.Length + 1;
                var relativePath = ParentPath.StartsWith(ThisPdeName.Name + "/")
                    ? ParentPath[startIndex..]
                    : ParentPath;
                var parentDir = GetOrCreateDir(relativePath);
                if (Type == 1) {
                    if (!DirStructureOffsets.ContainsKey("Files")) {
                        DirStructureOffsets["Files"] = new List<object>();
                    }
                    ((List<object>)parentDir["Files"]).Add(new {
                        Type,
                        Name,
                        BOffset = BOffset.ToString("X"),
                        OOffset = OOffset.ToString("X"),
                        Offset = Offset.ToString("X"),
                        Size = Size.ToString("X")
                    });
                } else {
                    parentDir[Name] = new Dictionary<string, object> {
                        ["_info"] = new {
                            Type,
                            Name,
                            BOffset = BOffset.ToString("X"),
                            OOffset = OOffset.ToString("X"),
                            Offset = Offset.ToString("X"),
                            Size = Size.ToString("X")
                        },
                        ["Files"] = new List<object>()
                    };
                }
            }
        }

        /// <summary>
        /// 获取或创建目录
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <returns>目录字典</returns>
        private static Dictionary<string, object> GetOrCreateDir(string relativePath) {
            var parts = relativePath.Split('/');
            var currentDir = DirStructureOffsets;
            foreach (var part in parts) {
                if (!string.IsNullOrEmpty(part)) {
                    if (!currentDir.ContainsKey(part)) {
                        currentDir[part] = new Dictionary<string, object> {
                            ["_info"] = new {
                                Type = 2,
                                Name = part,
                                BOffset = "0",
                                OOffset = "0",
                                Offset = "0",
                                Size = "0"
                            },
                            ["Files"] = new List<object>()
                        };
                    }
                    currentDir = (Dictionary<string, object>)currentDir[part];
                }
            }
            return currentDir;
        }

        /// <summary>
        /// 保存偏移值
        /// </summary>
        public static void Save() {
            // 使用 Newtonsoft.Json 以json格式覆盖保存 OffsetLogs 到目录中
            string DirStructureJson = Newtonsoft.Json.JsonConvert.SerializeObject(DirStructureOffsets);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ThisPdeName.Name, "DirStructureOffsets.json"), DirStructureJson);
            // 清空 DirStructureOffsets
            DirStructureOffsets.Clear();
            Console.WriteLine(" √偏移值已保存至:  " + ThisPdeName.Name + "/DirStructureOffsets.json");
        }
    }
}