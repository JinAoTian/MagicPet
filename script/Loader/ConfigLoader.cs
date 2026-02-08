using System.Collections.Generic;
using System.IO;
using desktop.script.logic;
using desktop.script.Util;
using Godot;
using FileAccess = Godot.FileAccess;


namespace desktop.script.Loader;

public static class ConfigLoader
{
    private const string 配置目录名 = "config";
    private static readonly Dictionary<string, Godot.Collections.Dictionary> ActiveProcesses = new();

    public static void 加载模组配置信息(string 路径)
    {
        var 配置目录 = Path.Combine(路径, 配置目录名);
        var 工具字典 = LoadUtil.FromJson<Dictionary<string, string>>(Path.Combine(配置目录, Main.工具配置文件名));
        if (工具字典!=null)
        {
            foreach (var (k,v) in 工具字典)
            {
                var 工具路径 = Path.Combine(路径, v);
                GD.Print(工具路径);
                if (File.Exists(工具路径))
                {
                    Main.工具路径字典[k] = 工具路径;
                }
                else
                {
                    Main.工具路径字典[k] = v;
                }
            }
        }
        加载配置信息(配置目录);
    }
    public static void 加载配置信息(string 路径)
    {
        if (!Directory.Exists(路径))return;
        var 初始化列表 = LoadUtil.FromJson<List<执行信息>>(Path.Combine(路径,Main.初始化配置文件名));
        if (初始化列表 != null)
        {
            foreach (var 初始化信息 in 初始化列表)
            {
                // 执行命令
                var result = OS.ExecuteWithPipe(
                    LoadUtil.GetExternalToolPath(初始化信息.tool), 
                    初始化信息.arguments
                );

                // 记录到字典中
                // result 包含了 "stdio" (FileAccess) 和 "pid" (int)
                if (result.TryGetValue("pid", out var value))
                {
                    ActiveProcesses[初始化信息.tool] = result;
                    GD.Print($"已启动工具: {初始化信息.tool}, PID: {value}");
                }
            }
        }
        var 配置信息 = LoadUtil.FromJson<Dictionary<string, string>>(Path.Combine(路径,Main.配置信息文件名));
        if (配置信息!=null)
        {
            foreach (var (k,v) in 配置信息)
            {
                Main.配置信息字典[k] = v;
            }
        }
    }
    public static void CleanupProcesses()
    {
        foreach (var toolName in ActiveProcesses.Keys)
        {
            var processData = ActiveProcesses[toolName];
            var pid = (int)processData["pid"];
            var pipe = (FileAccess)processData["stdio"];
            // 1. 关闭管道连接
            pipe.Close();

            // 2. 检查并杀死进程
            if (OS.IsProcessRunning(pid))
            {
                GD.Print($"正在强制关闭进程: {toolName} (PID: {pid})");
                OS.Kill(pid);
            }
        }
        ActiveProcesses.Clear();
    }
}