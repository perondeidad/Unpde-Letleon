using Unpde;
using static Unpde.DataType;
using static Unpde.Test;

/// <summary>
/// 解码 FC/AS 的 *.pde文件
/// By:letleon
/// 不可用于任何商业用途
/// 仅供学习交流使用
/// 如有侵权请联系删除
/// </summary>
class Program {
    static void Main() {
        Console.WriteLine(" ！解码 FC/AS 的 *.pde文件");

        // 获取目录下的PDE文件列表
        List<PdeNames> PdeList = GetPDEList.Get();

        // 遍历PdeList文件列表进行解密
        foreach (PdeNames pde in PdeList) {
            Console.WriteLine(" ！正在解码：" + pde.FullName);

            // 初始化PDETool
            PdeTool.Init(pde);

            // 获取170表
            // 由于170表目前不知是用在哪里，所以暂时注释掉
            //Tab170.Init();

            //测试用👇
            // 解密函数测试
            //TestDecryptFunc();
            //测试用👆

            // 开始解密目录
            // 创建目录结构
            DirStr TryDir = new() { UpDir = pde.Name + "/", NowDir = pde.Name + "/" };
            // 4096: 表示目录位置 1000H -> 4096(10进制)
            // 4096: 表示目录大小 1000H -> 4096(10进制)
            // false:表示目录或文件，true:表示170表
            Unpack.Try(4096, 4096, TryDir, false);
            // 解压后如果发现解压的数据大小不对，
            // 那么就说明还有目录没有被加压，
            // 就需要找到目录，使用下面的方法继续解压！

            // 测试用👇
            //// mesh\prop\weapon 0x2A360000 -> 708182016 ,大小 0x1000 -> 4096
            DirStr TryDir2 = new() { UpDir = "prop/", NowDir = pde.Name + "/mesh/prop/weapon" };
            //Unpack.Try(708182016, 4096, TryDir2, false);

            /////ui/LobbyUI 0x283CA000 -> 675061760 ,大小 0x1000 -> 4096
            //DirStr TryDir3 = new() { UpDir = "ui/", NowDir = pde.Name + "/ui/LobbyUI/" };
            //Unpack.Try(675061760, 4096, TryDir3, false);

            // 如果你找到了新的 目录地址 ，那么转换成10进制即可批量导出了！
            //Unpack.Try(1234567890, 4096, TryDir, false);
            // 测试用👆

            //调试用👇
            // TEST:计算偏移值
            //Test.CalcOffset("1");
            //return;

            // 保存 OffsetLogs 到pde目录
            OffsetLog.Save(pde.Name);

            // 保存 DebugPde 到Unpde目录
            // 有BUG懒得改
            //DebugPde.Save();
            //调试用👆

            Console.WriteLine(" √" + pde.FullName + " 解码完成");
            Console.WriteLine(" ！按任意键继续");
            Console.ReadKey();
            Console.WriteLine(" ");
        }

        Console.WriteLine(" √ PDE解码完成");
        Console.WriteLine(" ！按任意键退出");
        Console.ReadKey();
    }
}
