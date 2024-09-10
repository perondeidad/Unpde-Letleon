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

        /// <summary>
        /// 是否需要二次解密
        /// 需要:完成二次解密，后缀删除.cache 
        /// 不需要：不二次解密，直接输出原始文件,后缀带.cache
        /// </summary>
        public static bool NeedSecondaryDecryption = false;

        /// <summary>
        /// 当前正在处理的PDE文件名
        /// </summary>
        public static string NowPdeName = "";
    }
}
