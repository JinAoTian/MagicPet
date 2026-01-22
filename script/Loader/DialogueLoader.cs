using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using Godot;
namespace desktop.script.Loader;

public partial class DialogueLoader : Node
{    
    private const string 目录名 = "dialogue";
    private const string 入场招呼文件名 = "greetIn.json";
    private const string 离场招呼文件名 = "greetOut.json";
    private static readonly 对话信息 默认对话 = new();
    public static void 加载对话(string 路径)
    {
        var 总目录 = Path.Combine(路径, 目录名);
        if (!Directory.Exists(总目录))return;
        foreach (var 人物目录 in Directory.GetDirectories(总目录))
        {
            if(Main.IgnorePath(人物目录))continue;
            var 人物名 = Path.GetFileName(人物目录);
            var 对话信息 = 默认对话;
            if (人物名 != "default")
            {
                Main.人物字典.TryAdd(人物名, new 人物数据());
                对话信息 = Main.人物字典[人物名].对话信息;
            }
            对话信息.入场招呼列表 = LoadUtil.FromJson<List<招呼信息>>(Path.Combine(人物目录, 入场招呼文件名));
            对话信息.离场招呼列表 = LoadUtil.FromJson<List<招呼信息>>(Path.Combine(人物目录, 离场招呼文件名));
        }
    }
}
