using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unpde {
    /// <summary>
    /// 全局变量类
    /// </summary>
    internal class GVar {

        /// <summary>
        /// 是否需要记录调试PDE文件
        /// </summary>
        public static bool NeedDebugPde = false;

        /// <summary>
        /// 是否需要记录OffsetLogs文件
        /// </summary>
        public static bool NeedOffsetLog = false;

        /// <summary>
        /// 是否需要查找目录
        /// </summary>
        public static bool NeedFindDirs = false;

        /// <summary>
        /// PDE标识Hash表
        /// </summary>
        public static HashSet<string> TagsHash = [];


    }
}
