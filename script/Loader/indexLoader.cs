using System;
using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using Godot;

namespace desktop.script.Loader;

public static class IndexLoader
{
    private static string 脚本文件名 => $"{Main.执行函数名}.gd";
    private const string 抬头文件名 = "header.json";
    private const string 文件目录名 = "file";
    public static readonly Dictionary<string, List<索引脚本信息>> 文件脚本映射 = new();
    public static readonly Dictionary<string, 抬头信息> 文件索引映射 = new();//扩展名
    public static readonly List<索引脚本信息> 文件通用脚本列表 = new();
    private const string 网址目录名 = "web";
    public static readonly Dictionary<string, List<索引脚本信息>> 网址脚本映射 = new();
    public static readonly Dictionary<string, 抬头信息> 网址索引映射 = new();//扩展名
    public static readonly List<索引脚本信息> 网址通用脚本列表 = new();
    public static void 加载索引脚本(string 路径)
    {
        var 文件目录 = Path.Combine(路径, 文件目录名);
        if (Directory.Exists(文件目录))
        {
            加载抬头(文件目录,文件索引映射);
            加载脚本(文件目录, 文件脚本映射, 文件通用脚本列表);
        }        
        var 网址目录 = Path.Combine(路径, 网址目录名);
        if (Directory.Exists(网址目录))
        {
            加载抬头(网址目录,网址索引映射);
            加载脚本(网址目录, 网址脚本映射, 网址通用脚本列表);
        }
    }
    private static void 加载脚本(string 总目录,Dictionary<string, List<索引脚本信息>> 脚本映射,List<索引脚本信息>通用脚本列表)
    {
        foreach (var 脚本目录 in Directory.GetDirectories(总目录))
        {
            if(Main.IgnorePath(脚本目录))continue;
            var 配置文件路径 = Path.Combine(脚本目录, Main.配置文件名);
            var 脚本文件路径 = Path.Combine(脚本目录, 脚本文件名);
            if (File.Exists(配置文件路径) && File.Exists(脚本文件路径))
            {
                try
                {
                    var info = LoadUtil.FromJson<索引脚本信息>(配置文件路径);
                    info.Path = 脚本目录;
                    info.LoadIcon();
                    if (info.extensions!=null)
                    {
                        foreach (var 索引 in info.extensions)
                        {
                            脚本映射.TryAdd(索引, []);
                            脚本映射[索引].Add(info);
                        }
                    }
                    else
                    {
                        通用脚本列表.Add(info);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"解析配置文件失败: {配置文件路径}, 错误: {ex.Message}");
                }
            }
        }
    }

    private static void 加载抬头(string 总目录,Dictionary<string, 抬头信息>索引映射)
    {
        var 抬头路径 = Path.Combine(总目录, 抬头文件名);
        if(!File.Exists(抬头路径))return;
        var 抬头列表 = LoadUtil.FromJson<List<抬头信息>>(抬头路径);
        if (抬头列表==null)return;
        foreach (var 抬头信息 in 抬头列表)
        {
            foreach (var 扩展名 in 抬头信息.extensions)
            {
                索引映射[扩展名] = 抬头信息;
            }
        }
    }
}

