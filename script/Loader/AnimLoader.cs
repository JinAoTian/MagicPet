using System;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using Godot;


namespace desktop.script.Loader;

public static class AnimLoader
{
    private const string 目录名 = "anim";
    public static void 加载动画(string 路径)
    {
        var 总目录 = Path.Combine(路径, 目录名);
        if (!Directory.Exists(总目录)) return;
        // 1. 遍历第一级目录：人物
        foreach (var 人物目录 in Directory.GetDirectories(总目录))
        {
            if(Main.IgnorePath(人物目录))continue;
            var 人物名 = Path.GetFileName(人物目录);
            ModLoader.人物字典.TryAdd(人物名, new 人物数据());
            var 人物 = ModLoader.人物字典[人物名];
            // 2. 遍历第二级目录：动画类型 
            foreach (var 类型目录 in Directory.GetDirectories(人物目录))
            {
                if(Main.IgnorePath(类型目录))continue;
                var 类型名 = Path.GetFileName(类型目录);
                人物.动画池字典.TryAdd(类型名, []);
                // 3. 遍历第三级目录：动画名
                foreach (var 动画名目录 in Directory.GetDirectories(类型目录))
                {
                    // 4. 寻找第四级目录中的配置文件
                    if(Main.IgnorePath(动画名目录))continue;
                    var 配置文件路径 = Path.Combine(动画名目录, Main.配置文件名);
                    if (File.Exists(配置文件路径))
                    {
                        try
                        {
                            var 动画名 = $"{类型名}-{Path.GetFileName(动画名目录)}";
                            var info = LoadUtil.FromJson<动画信息>(配置文件路径);
                            info.Name = 动画名;
                            info.Path = 动画名目录;
                            info.Type = 类型名;
                            人物.动画信息映射[动画名] = info;
                            人物.动画池字典[类型名].Add(info);
                        }
                        catch (Exception ex)
                        {
                            GD.PrintErr($"解析配置文件失败: {配置文件路径}, 错误: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
