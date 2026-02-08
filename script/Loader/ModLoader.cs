using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Steam;
using desktop.script.Util;
using Godot;

namespace desktop.script.Loader;

public static class ModLoader
{
    // 假设 Key 是路径或动画名，Value 是解析后的某些数据（或保留原始映射）
    public static Dictionary<string, 人物数据> 人物字典 => Main.人物字典;
    private const string 模组识别文件 = "magick.pet";
    public static string 模组路径;
    public static void 加载模组()
    {
        遍历模组目录(LoadUtil.ModPath,false);
        遍历模组目录(OS.GetExecutablePath().GetBaseDir().GetBaseDir(),true);//steam加载逻辑
        遍历模组目录(WorkShop.GetWorkshopParentPath(),false);
    }
    private static void 遍历模组目录(string 目录,bool 检测识别)
    {
        if(string.IsNullOrEmpty(目录))return;
        foreach (var 模组目录 in Directory.GetDirectories(目录))
        {
            if(Main.IgnorePath(模组目录))continue;
            if(检测识别 && !File.Exists(Path.Combine(模组目录,模组识别文件)))continue;
            加载模组(模组目录);
        }
    }
    private static void 加载模组(string 路径)
    {
        模组路径 = 路径;
        ConfigLoader.加载模组配置信息(路径);
        AnimLoader.加载动画(路径);
        IndexLoader.加载索引脚本(路径);
        CommandLoader.加载指令脚本(路径);
        NodeLoader.加载场景列表(路径);
        DialogueLoader.加载对话(路径);
        LoadUtil.LoadI18nCSV(Path.Combine(路径,Main.本地化文件名));
    }
}
