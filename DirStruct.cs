using System;
using System.IO;
using Newtonsoft.Json;

namespace Unpde {
    /// <summary>
    /// 读取附加的目录结构文件
    /// </summary>
    public static class DirStruct {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="Version"></param>
        public static uint[] Read(String Version) {
            Console.WriteLine(Version);

            // 声明数组变量
            uint[] DirStructList = [];

            string JsonPath = Path.Combine(Directory.GetCurrentDirectory(), "DirStruct", $"offset_{Version}.json");
            Console.WriteLine(JsonPath);

            if (File.Exists(JsonPath)) {
                try {
                    string Json = File.ReadAllText(JsonPath);
                    DirStructList = JsonConvert.DeserializeObject<uint[]>(Json) ?? []; // 正确初始化空数组

                } catch (Exception ex) {
                    Console.WriteLine("解析JSON文件时出错: " + ex.Message);
                }
            } else {
                Console.WriteLine("未找到目录结构文件");
            }

            return DirStructList;
        }
    }
}