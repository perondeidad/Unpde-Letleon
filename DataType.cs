using Newtonsoft.Json;

namespace Unpde {
    public static class DataType {
        /// <summary>
        /// 170数据块结构
        /// </summary>
        public class Table170 {
            /// <summary>
            /// 块大小
            /// </summary>
            [JsonProperty("size")]
            public required uint Size { get; set; }
            /// <summary>
            /// 块在PDE文件中的地址
            /// </summary>
            [JsonProperty("offset")]
            public required uint Offset { get; set; }
        }

        /// <summary>
        /// 170数据块结构日志版
        /// </summary>
        public class Table170LogVer {
            /// <summary>
            /// 块大小
            /// </summary>
            [JsonProperty("size")]
            public required string Size { get; set; }
            /// <summary>
            /// 块在PDE文件中的地址
            /// </summary>
            [JsonProperty("offset")]
            public required string Offset { get; set; }
        }

        /// <summary>
        /// 文件偏移信息数据结构
        /// </summary>
        public class HexOffsetInfo {
            /// <summary>
            /// 1 文件,2 目录
            /// </summary>
            public required uint Type { get; set; }
            /// <summary>
            /// 文件名或目录名
            /// </summary>
            public required string Name { get; set; }
            /// <summary>
            /// 偏移值
            /// </summary>
            public required uint Offset { get; set; }
            /// <summary>
            /// 大小
            /// </summary>
            public required uint Size { get; set; }
        }

        /// <summary>
        /// 目录数据结构
        /// </summary>
        public class DirStr {
            /// <summary>
            /// 上级目录
            /// </summary>
            public required string UpDir { get; set; }
            /// <summary>
            /// 当前目录
            /// </summary>
            public required string NowDir { get; set; }
        }

        /// <summary>
        /// OffsetJson数据结构
        /// </summary>
        public class OffsetJsonStr {
            /// <summary>
            /// 块大小
            /// </summary>
            [JsonProperty("size")]
            public required List<uint> Size { get; set; }
            /// <summary>
            /// 块在PDE文件中的地址
            /// </summary>
            [JsonProperty("offset")]
            public required uint Offset { get; set; }
        }

        /// <summary>
        /// GetByteOfPde() 返回的数据结构 
        /// </summary>
        public class GetOffsetStr {
            /// <summary>
            /// 实际获取到的块大小
            /// </summary>
            public required uint Size { get; set; }
            /// <summary>
            /// 获取到的Byte[]
            /// </summary>
            public required byte[] Byte { get; set; }
        }

        public class PdeNames {
            public required string Name { get; set; }
            public required string FullName { get; set; }
        }

    }
}
