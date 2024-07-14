using Newtonsoft.Json;

namespace Unpde {
    /// <summary>
    /// 数据类型定义
    /// </summary>
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
            /// 在PDE文件中的实际偏移值
            /// </summary>
            public required uint Offset { get; set; }
            /// <summary>
            /// 大小
            /// </summary>
            public required uint Size { get; set; }
            /// <summary>
            /// 原始偏移值
            /// </summary>
            public required uint OOffset { get; set; }

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

        /// <summary>
        /// PDE文件名结构
        /// </summary>
        public class PdeNames {
            /// <summary>
            /// 文件名
            /// </summary>
            public required string Name { get; set; }
            /// <summary>
            /// 文件全名
            /// </summary>
            public required string FullName { get; set; }
        }

        /// <summary>
        /// 解码后的JSON数据目录结构
        /// </summary>
        public class DecodedDirJson {
            /// <summary>
            /// 文件或文件夹类型
            /// </summary>
            public required int Type { get; set; }
            /// <summary>
            /// 文件或文件夹名称
            /// </summary>
            public required string Name { get; set; }
            /// <summary>
            /// 数据块在PDE文件中的偏移值
            /// </summary>
            public required string BOffset { get; set; }
            /// <summary>
            /// 原始偏移值
            /// </summary>
            public required string OOffset { get; set; }
            /// <summary>
            /// 真实偏移值
            /// </summary>
            public required string Offset { get; set; }
            /// <summary>
            /// 大小
            /// </summary>
            public required string Size { get; set; }
        }

        /// <summary>
        /// 未解码的目录结构
        /// </summary>
        public class UnDecodedDir {
            /// <summary>
            /// 文件或文件夹类型
            /// </summary>
            public required long OffSet { get; set; }
            /// <summary>
            /// 大小
            /// </summary>
            public required long Size { get; set; }
        }
    }
}
