// Code by claude-3-opus-20240229
namespace Unpde {
    /// <summary>
    /// 偏移值信息记录
    /// </summary>
    internal class OffsetLog {
        /// <summary>
        /// 目录结构偏移值信息
        /// </summary>
        private static readonly Dictionary<string, object> DirStructureOffsets = [];

        /// <summary>
        /// 记录偏移值
        /// </summary>
        public static void Rec(uint Offset, uint Size, string Name, uint Type, string ParentPath) {
            // 记录目录结构偏移值信息
            if (string.IsNullOrEmpty(ParentPath)) {
                if (Type == 1) {
                    ((List<object>)DirStructureOffsets["Files"]).Add(new {
                        Type,
                        Name,
                        Offset = Offset.ToString("X"),
                        Size = Size.ToString("X")
                    });
                } else {
                    DirStructureOffsets[Name] = new Dictionary<string, object> {
                        ["_info"] = new {
                            Type,
                            Name,
                            Offset = Offset.ToString("X"),
                            Size = Size.ToString("X")
                        },
                        ["Files"] = new List<object>()
                    };
                }
            } else {
                var relativePath = ParentPath.StartsWith("Unpde/") ? ParentPath[6..] : ParentPath;
                var parentDir = GetOrCreateDir(relativePath);
                if (Type == 1) {
                    if (!DirStructureOffsets.ContainsKey("Files")) {
                        DirStructureOffsets["Files"] = new List<object>();
                    }
                    ((List<object>)parentDir["Files"]).Add(new {
                        Type,
                        Name,
                        Offset = Offset.ToString("X"),
                        Size = Size.ToString("X")
                    });
                } else {
                    parentDir[Name] = new Dictionary<string, object> {
                        ["_info"] = new {
                            Type,
                            Name,
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
        public static void Save(string Name) {
            // 使用 Newtonsoft.Json 以json格式覆盖保存 OffsetLogs 到Unpde目录中
            string DirStructureJson = Newtonsoft.Json.JsonConvert.SerializeObject(DirStructureOffsets);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Name, "DirStructureOffsets.json"), DirStructureJson);
            Console.WriteLine(" √偏移值已保存至:  " + Name + "/DirStructureOffsets.json");
        }
    }
}