using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;

namespace desktop.script.Loader;

public static class ConfigLoader
{
    private const string 目录名 = "config";

    public static void 加载配置信息(string 路径)
    {
        var 总目录 = Path.Combine(路径, 目录名);
        if (!Directory.Exists(总目录))return;
        var 工具字典路径 =  Path.Combine(总目录, Main.工具配置文件名);
        var 工具字典 = LoadUtil.FromJson<Dictionary<string, string>>(工具字典路径);
        if (工具字典!=null)
        {
            foreach (var (k,v) in 工具字典)
            {
                var 工具路径 = Path.Combine(路径, v);
                if (File.Exists(工具路径))
                {
                    Main.工具路径字典[k] = 工具路径;
                }
            }
        }
    }
}