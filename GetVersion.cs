using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Unpde.DataType;

namespace Unpde {
    /// <summary>
    /// 获取PDE版本号
    /// </summary>
    public static class GetVersion {
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns></returns>
        public static String Get() {
            // 版本号
            String PDEVersion = "";

            // 读取数据
            DataType.GetOffsetStr TryByte = PdeTool.GetByteOfPde(4096, 4096);
            // 解密数据块 -> 同时生成一个供调试时使用的PDE文件
            byte[] DeTryByte = PdeTool.DeFileOrBlock(TryByte.Byte, false);
            // 获取文件或文件夹偏移信息
            List<HexOffsetInfo> DeTryByteArr = PdeTool.GetOffsetInfo(DeTryByte, 4096);

            // 获取版本信息
            foreach (HexOffsetInfo Item in DeTryByteArr) {
                if (Item.Name == "version") {
                    Console.WriteLine("找到Version");
                    //获取指定偏移的字节数据
                    GetOffsetStr TempFileByte = PdeTool.GetByteOfPde(Item.Offset, Item.Size);
                    //解密数据
                    byte[] DeTempFileByte = PdeTool.DeFileOrBlock(TempFileByte.Byte, false);
                    PDEVersion = Encoding.Default.GetString(DeTempFileByte);
                    //打印DeTempFileByte为字符串
                    Console.WriteLine(PDEVersion);
                    break;
                }
            }

            // 返回版本信息
            return PDEVersion;
        }
    }
}
