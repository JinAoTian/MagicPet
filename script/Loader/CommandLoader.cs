using System;
using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using Godot;

namespace desktop.script.Loader;

public static class CommandLoader
{

    private static string 脚本文件名 => $"{Main.执行函数名}.gd";
    private const string 直接目录名 = "command";
    public static readonly List<脚本信息> 直接指令列表 = new();
    public static readonly Dictionary<string, 脚本组信息> 直接指令组映射 = new();
    public static readonly Dictionary<string, List<脚本信息>> 直接指令组脚本映射 = new();    
    private const string 文本目录名 = "txt";
    public static readonly List<脚本信息> 文本指令列表 = new();
    private static readonly Dictionary<string, 脚本组信息> 文本指令组映射 = new();
    private static readonly Dictionary<string, List<脚本信息>> 文本指令组脚本映射 = new();
    private const string 展示目录名 = "showOut";
    public static readonly List<脚本信息> 展示指令列表 = new();
    private static readonly Dictionary<string, 脚本组信息> 展示指令组映射 = new();
    private static readonly Dictionary<string, List<脚本信息>> 展示指令组脚本映射 = new();
    public static void 加载指令脚本(string 路径)
    {
        加载指令(Path.Combine(路径, 直接目录名),直接指令列表,直接指令组映射,直接指令组脚本映射);
        加载指令(Path.Combine(路径, 文本目录名),文本指令列表,文本指令组映射,文本指令组脚本映射);
        加载指令(Path.Combine(路径, 展示目录名),展示指令列表,展示指令组映射,展示指令组脚本映射);
    }
    private static void 加载指令(string 总目录,List<脚本信息> 指令列表,Dictionary<string, 脚本组信息>指令组映射,Dictionary<string, List<脚本信息>>指令组脚本映射)
    {
        if (!Directory.Exists(总目录)) return;
        var 脚本组列表 = LoadUtil.FromJson<List<脚本组信息>>(Path.Combine(总目录, Main.脚本组文件名));
        if (脚本组列表!=null)
        {
            foreach (var 脚本组信息 in 脚本组列表)
            {
                指令组映射[脚本组信息.id] = 脚本组信息;
                if (!string.IsNullOrEmpty(脚本组信息.icon) && ImageUtil.Loadimage(Path.Combine(总目录,脚本组信息.icon),out var image))
                {
                    脚本组信息.IconImg = image;
                }
            }
        }
        foreach (var 脚本目录 in Directory.GetDirectories(总目录))
        {
            if(Main.IgnorePath(脚本目录))continue;
            var 配置文件路径 = Path.Combine(脚本目录, Main.配置文件名);
            var 脚本文件路径 = Path.Combine(脚本目录, 脚本文件名);
            if (File.Exists(配置文件路径) && File.Exists(脚本文件路径))
            {
                try
                {
                    var info = LoadUtil.FromJson<脚本信息>(配置文件路径);
                    if(info==null)continue;
                    info.Path = 脚本目录;
                    info.ModPath = ModLoader.模组路径;
                    info.LoadIcon();
                    if (string.IsNullOrEmpty(info.group))
                    {
                        指令列表.Add(info);
                    }
                    else
                    {
                        指令组脚本映射.TryAdd(info.group, []);
                        指令组脚本映射[info.group].Add(info);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"解析配置文件失败: {配置文件路径}, 错误: {ex.Message}");
                }
            }
        }
    }
}