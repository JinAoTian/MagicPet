using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using desktop.script.UX;
using Godot;

namespace desktop.script.Loader;

public partial class ModLoader : Node
{
    // 假设 Key 是路径或动画名，Value 是解析后的某些数据（或保留原始映射）
    public static Dictionary<string, 人物数据> 人物字典 => Main.人物字典;
    public override void _Ready()
    {
        foreach (var 模组目录 in Directory.GetDirectories(Main.ModPath))
        {
            if(Main.IgnorePath(模组目录))continue;
            加载模组(模组目录);
        }
        Context.显示指令列表();
        CharAnim.载入人物动画();

    }
    private static void 加载模组(string 路径)
    {
        AnimLoader.加载动画(路径);
        IndexLoader.加载索引脚本(路径);
        CommandLoader.加载指令脚本(路径);
        NodeLoader.加载场景列表(路径);
        LoadUtil.LoadI18nCSV(Path.Combine(路径,Main.本地化文件名));
    }
}
