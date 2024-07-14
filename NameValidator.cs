using System.Text.RegularExpressions;

namespace Unpde {
    /// <summary>
    /// 名称验证器
    /// </summary>
    internal partial class NameValidator {

        // 正则表达式，用于匹配Windows文件名中不允许的字符  
        [GeneratedRegex("[<>:\"/\\|?*]")]
        private static partial Regex NameRegex();
        private static readonly Regex InvalidFileNameChars = NameRegex();

        /// <summary>
        /// 检查
        /// </summary>
        /// <param name="Type">类型</param>
        /// <param name="Name">名称</param>
        /// <returns>是否合法</returns>
        public static bool Check(uint Type, string Name) {
            bool Result = false;
            if (Type == 1) {
                // 判断文件名是否合法
                if (FileNmae(Name)) Result = true;
            } else if (Type == 2) {
                // 判断目录名是否合法
                if (DirectoryName(Name)) Result = true;
            }
            return Result;
        }

        /// <summary>
        /// 检查文件名是否合法
        /// </summary>
        /// <param name="fileName">文件名 </param>
        /// <returns>是否合法 </returns>
        private static bool FileNmae(string fileName) {
            // 文件名不能为空  
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // 文件名不能包含Windows不允许的字符  
            if (InvalidFileNameChars.IsMatch(fileName))
                return false;

            // 文件名不能以.或..开头  
            if (fileName.StartsWith('.') || fileName.StartsWith(".."))
                return false;

            // 文件名长度不能超过255个字符  
            if (fileName.Length > 255)
                return false;

            // 文件名不能包含 除了 _ - 之外的符号
            if (!fileName.All(c => c == '_' || c == '-' || c == '\\' || c == '"' || c == '.' || (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                return false;


            return true;
        }

        /// <summary>
        /// 检查文件夹名是否合法
        /// </summary>
        /// <param name="dirName">文件夹名 </param>
        /// <returns>是否合法 </returns>
        private static bool DirectoryName(string dirName) {
            // 文件夹名不能为空  
            if (string.IsNullOrWhiteSpace(dirName))
                return false;

            // 文件夹名不能包含Windows不允许的字符，并且不能以.或..开头  
            if (!FileNmae(dirName))
                return false;

            // 文件夹名不能包含以下字符，因为它们在路径中有特殊含义  
            if (dirName.Contains('\\') || dirName.Contains('/'))
                return false;

            // 文件夹名长度不能超过255个字符（与文件名相同限制）  
            if (dirName.Length > 255)
                return false;

            // 文件名不能包含 除了 _ - 之外的符号
            if (!dirName.All(c => c == '_' || c == '-' || c == '\\' || c == '"' || c == '.' || (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                return false;

            return true;
        }
    }
}
