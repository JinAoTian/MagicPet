using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using desktop.script.Util;
using desktop.script.UX;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace desktop.script.logic;
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
    public string tool;
    public List<string> tools;
    public string tip;
    public bool option;
    public bool excute=true;
    public bool cheer = true;
    public bool prepare;
    public bool wait;
    public bool showOut;
    public string Path;
    public string ModPath;
}
public class 关键词脚本信息 : 脚本信息
{
    public List<关键词信息> keywordList;
}
public class 可见脚本信息:脚本信息
{
    public string name;
    public string group;
    // ReSharper disable once MemberCanBePrivate.Global
    public string icon = "icon.png";
    public string config;
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

public class 目录脚本信息 : 可见脚本信息
{
    public bool multi;//支持多个目录
}
public class 索引脚本信息:可见脚本信息
{
    public bool batch;//可批处理
    public bool multi;//支持多个扩展名混用
    public List<string> extensions;
}
public class 人物数据
{
    public readonly Dictionary<string, List<动画信息>> 动画池字典=new();
    public readonly Dictionary<string, 动画信息> 动画信息映射=new();
    public readonly 对话信息 对话信息 = new();
}
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
public class 关键词信息
{
    public List<string> keywords;
    public Dictionary<string, string> info;
    [JsonIgnore] public 关键词脚本信息 对应脚本;
}
public class 对话信息
{
    public List<招呼信息> 入场招呼列表;
    public List<招呼信息> 离场招呼列表;
    public string 入场招呼 => 招呼信息.获取招呼(入场招呼列表);
    public string 离场招呼 => 招呼信息.获取招呼(离场招呼列表);

// 从 text 列表中随机取出一句
}
public class 招呼信息
{
    public List<int> time;//格式 [1130,1230]表示 11:30 ~ 12:30
    public int priority;//优先级
    public int weight;//权重
    public List<string> text;
    public static string 获取招呼(List<招呼信息> 招呼列表)
    {
        if (招呼列表 == null || 招呼列表.Count == 0) return null;
        // 1. 获取当前时间（格式为 HHmm）
        var currentTime = int.Parse(DateTime.Now.ToString("HHmm"));
        // 2. 第一轮过滤：时间匹配
        var matches = 招呼列表.Where(info => 
        {
            if (info.time == null || info.time.Count < 2) return true; // 通用内容
            return currentTime >= info.time[0] && currentTime <= info.time[1];
        }).ToList();

        if (matches.Count == 0) return null;

        // 3. 第二轮过滤：只保留最高优先级的一组
        var maxPriority = matches.Max(m => m.priority);
        var priorityGroup = matches.Where(m => m.priority == maxPriority).ToList();

        // 4. 第三轮：加权随机
        var totalWeight = priorityGroup.Sum(m => m.weight);
        if (totalWeight <= 0) return priorityGroup[0].text.列表随机项();

        var randomRoll = new Random().Next(0, totalWeight);
        var currentSum = 0;

        foreach (var info in priorityGroup)
        {
            currentSum += info.weight;
            if (randomRoll < currentSum)
            {
                return info.text.列表随机项();
            }
        }
        return null;
    }
}