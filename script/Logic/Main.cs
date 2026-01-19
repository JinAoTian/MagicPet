using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using desktop.script.Util;
using desktop.script.UX;
using Godot;
using DialogueManagerRuntime;

// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace desktop.script.logic;
public partial class Main:Node
{
    public const string 执行函数名 = "execute";
    public const string 本地化文件名 = "i18n.csv";
    public const string 配置文件名 = "info.json";
    public const string 脚本组文件名 = "group.json";
    public const string 工具配置文件名 = "tool.json";
    public const string 初始化配置文件名 = "init.json";
    private const string 预处理函数名 = "prepare";
    private const string 对话文件名 = "option.dialogue";
    public const string 配置信息文件名 = "config.json";
    public static readonly Dictionary<string, 人物数据> 人物字典 = new();
    public static 人物数据 显示人物 => 人物字典[_配置信息字典.GetValueOrDefault("当前人物","loris")];
    public static List<脚本信息> 配置脚本列表 = [];
    public static Dictionary<string, string> 工具路径字典=new();
    public static Dictionary<string, string> _配置信息字典=new();
    private static Main _单例;
    public static 脚本信息 当前脚本;
    public override void _Ready()
    {
        _单例 = this;
        LoadUtil.初始化();
    }
    public static bool IgnorePath(string path) => Path.GetFileName(path).StartsWith("_");
    public static void 选择脚本(脚本信息 脚本信息)
    {
        当前脚本 = 脚本信息;
        IO.单例.set("tool",LoadUtil.GetExternalToolPath(脚本信息.tool));
        IO.单例.set("script",脚本信息.Path);
        IO.单例.set("mod",脚本信息.ModPath);
        IO.单例.set("out",LoadUtil.GetOutputDir());
        if (脚本信息.option)
        {
            if (脚本信息.prepare)
            {
                运行预处理函数(脚本信息);
            }
            else
            {
                预处理函数完成();
            }
        }
        else
        {
            运行执行函数(脚本信息);
        }
    }
    public static void 打开配置(int index)
    {
        if(index>=配置脚本列表.Count)return;
        var 配置路径 = 配置脚本列表[index].config;
        var absolutePath = ProjectSettings.GlobalizePath(配置路径);
        if (OS.GetName() == "Windows")
        {
            // Windows: 使用 powershell 或 cmd 调用 start 指令
            OS.Execute("cmd.exe", ["/C", "start", "", absolutePath]);
        }
        else if (OS.GetName() == "macOS")
        {
            // macOS: 使用 open 命令
            OS.Execute("open", [absolutePath]);
        }
        else if (OS.GetName() == "X11") // Linux
        {
            // Linux: 使用 xdg-open 命令
            OS.Execute("xdg-open", [absolutePath]);
        }
        CharAnim.开始庆祝();
    }
    private static async void 加载对话(string absolutePath)
    {
        try
        {
            if (!File.Exists(absolutePath)) return;
            var rawText = await File.ReadAllTextAsync(absolutePath);
            var temporaryResource = DialogueManager.CreateResourceFromText(rawText);
            Dialogue.开始对话(temporaryResource);
        }
        catch (Exception e)
        {
            GD.Print($"对话读取错误:[{e.Message}]");
        }
    }
    public static void 对话结束() => 运行执行函数(当前脚本);
    private static void 运行执行函数(脚本信息 脚本信息)
    {
        if (脚本信息.wait)
        {
            Dialogue.显示标题("wait"); 
        }

        // 直接调用通用提取函数
        RunScriptTask(脚本信息, 执行函数名, "执行函数完成");
        Dialogue.脚本结束();
    }
    private static void 执行函数完成()
    {
        CharAnim.开始庆祝();
        var tip = string.IsNullOrEmpty(当前脚本.tip) ? "done" : 当前脚本.tip;
        if (IO.单例.get("tip", out var value))
        {
            tip = (string)value;
        }
        if (当前脚本.showOut)
        {
            Dialogue.文件处理完成(tip);
        }
        else
        {
            if (tip == "done")
            {
                Dialogue.关闭标题();
            }
            else
            {
                _ = Dialogue.显示临时标题(tip);
            }
        }
        IO.单例.Info.Clear();
    }
    private static void 运行预处理函数(脚本信息 脚本信息)
    {
        // 直接调用通用提取函数
        RunScriptTask(脚本信息, 预处理函数名, "预处理函数完成");
    }
    private static void 预处理函数完成()
    {
        加载对话(Path.Combine(当前脚本.Path,对话文件名));
    }
    public static void 注册脚本信息(脚本信息 脚本信息)
    {
        脚本信息.LoadIcon();
        if (!string.IsNullOrEmpty(脚本信息.config))
        {
            脚本信息.config = Path.Combine(脚本信息.Path,脚本信息.config);
            配置脚本列表.Add(脚本信息);
        }
    }
    private static void RunScriptTask(脚本信息 脚本信息, string gdMethodName, string callbackMethodName)
    {
        // 1. 加载并实例化脚本
        var scriptPath = Path.Combine(脚本信息.Path, $"{gdMethodName}.gd");
        var 脚本 = GD.Load<GDScript>(scriptPath);
        var 脚本实例 = (GodotObject)脚本.New();
        // 2. 异步执行任务
        Task.Run(() => 
        {
            try 
            {
                脚本实例.Call(gdMethodName, IO.单例.Info);
                _单例.CallDeferred(callbackMethodName);
            }
            catch (Exception e)
            {
                GD.PrintErr($"执行脚本 {gdMethodName} 时出错: {e.Message}");
            }
        });
    }
}
//对外暴露的字段统一用小驼峰敖
public class 脚本组信息
{
    public string id;
    public string name;
    public string ask;
    public string icon;
    public Texture2D IconImg;
}
public class 脚本信息
{
    public string name;
    public string tool;
    public string group;
    public string tip;
    // ReSharper disable once MemberCanBePrivate.Global
    public string icon = "icon.png";
    public bool option;
    public bool prepare;
    public bool wait;
    public bool showOut;
    public string config;
    public string Path;
    public string ModPath;
    public Texture2D IconImg;
    public void LoadIcon()
    {
        if (string.IsNullOrEmpty(icon) || !ImageUtil.Loadimage(System.IO.Path.Combine(Path,icon),out var image))
        {
            IconImg = null;
        }
        else
        {
            IconImg = image;
        }
    }
}
// ReSharper disable once ClassNeverInstantiated.Global
public class 索引脚本信息:脚本信息
{
    public bool batch;//可批处理
    public bool multi;//支持多个扩展名混用
    public List<string> extensions;
}
public class 人物数据
{
    public readonly Dictionary<string, List<动画信息>> 动画池字典=new();
    public readonly Dictionary<string, 动画信息> 动画信息映射=new();
}
// ReSharper disable once ClassNeverInstantiated.Global
public class 抬头信息
{
    public List<string> extensions;
    public string name;
    public bool batch;//可批处理
    public bool multi;//支持多个扩展名混用
}
public class 动画信息
{
    public int rate;
    public string name;
    public List<string> nextClip;
    public string Type;
    public string Path;
}

public class 初始化信息
{
    public string tool;
    public string[] arguments = [];
}