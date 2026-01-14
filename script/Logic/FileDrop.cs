using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using desktop.script.Loader;
using desktop.script.UX;
using Godot;
using Godot.Collections;

// 引入异步命名空间
namespace desktop.script.logic;

public partial class FileDrop : Node
{
    public static FileDrop _单例;
    public override void _Ready()
    {
        GetTree().Root.FilesDropped += OnFilesDropped;
        _单例 = this;
    }

    public static void DropFiles(string[] files) => _单例.OnFilesDropped(files);
    private void OnFilesDropped(string[] files)
    {
        // 关键点 1: 立即让窗口请求焦点
        DisplayServer.WindowMoveToForeground();

        // 关键点 2: 异步执行逻辑，不要阻塞当前的信号回调
        // 尤其是包含 OS.Execute 这种耗时操作时
        _ = ProcessDroppedFilesAsync(files);
    }

    private async Task ProcessDroppedFilesAsync(string[] files)
    {
        var paths = new List<string>();
        var relativePaths = new List<string>();
        // 在后台线程或异步任务中处理 IO 耗时逻辑
        await Task.Run(() => {
            foreach (var path in files)
            {
                var finalPath = path.ToLower();
                if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                {
                    finalPath = GetShortcutTarget(path);
                }

                if (Directory.Exists(finalPath))
                {
                    // 获取 finalPath 的上一级目录路径
                    var parentPath = Directory.GetParent(finalPath)?.FullName;
                    if (parentPath != null)
                    {
                        var subFiles = Directory.GetFiles(finalPath, "*.*", SearchOption.AllDirectories);
        
                        foreach (var file in subFiles)
                        {
                            var attributes = File.GetAttributes(file);
                            if ((attributes & FileAttributes.Hidden) == 0 && (attributes & FileAttributes.System) == 0)
                            {
                                paths.Add(file);
                
                                // 计算相对于父目录的路径
                                // 例如：如果 parentPath 是 C:\Projects，file 是 C:\Projects\MyApp\data.txt
                                // 结果将是 "MyApp\data.txt"
                                relativePaths.Add(Path.GetRelativePath(parentPath, file));
                            }
                        }
                    }
                }
                else if (File.Exists(finalPath))
                {
                    paths.Add(finalPath);
                    relativePaths.Add(finalPath.GetFile());
                    GD.Print(finalPath.GetFile());
                }
            }
        });

        var extension = Path.GetExtension(paths[0]);
        IO.单例.set("in",paths.ToArray());
        IO.单例.set("inR",relativePaths.ToArray());
        IO.单例.set("ext",extension);
        IO.单例.set("single",paths.Count == 1);
        // 关键点 3: 使用 CallDeferred 或等待一帧后再触发 UI
        // 这确保了操作系统的“拖拽释放”事件已经完全结束
        CallDeferred(nameof(FinalizeProcess), Variant.From(paths.ToArray()));
    }

    private static void FinalizeProcess(string[] paths)
    {
        Main._单例.处理路径(paths,IndexLoader.文件索引映射,IndexLoader.文件脚本映射,IndexLoader.文件通用脚本列表,"file-ask");
    }
    [SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
    private static string GetShortcutTarget(string lnkPath)
    {
        try
        {
            // 创建 COM 类型
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            // ReSharper disable once AssignNullToNotNullAttribute
            dynamic shell = Activator.CreateInstance(shellType);
        
            // 创建快捷方式对象
            // ReSharper disable once PossibleNullReferenceException
            var shortcut = shell.CreateShortcut(lnkPath);
            string targetPath = shortcut.TargetPath;
        
            // 释放 COM 资源
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
        
            return string.IsNullOrEmpty(targetPath) ? lnkPath : targetPath;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"解析快捷方式失败: {ex.Message}");
            return lnkPath;
        }
    }
}