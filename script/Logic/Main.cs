using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using desktop.script.Loader;
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
    private const string 配置信息文件名 = "config.json";
    private static string BinPath => GetProjectPath("bin");
    private static string ConfigPath => GetProjectPath("config");
    public static string ModPath => GetProjectPath("mods");
    public static readonly Dictionary<string, 人物数据> 人物字典 = new();
    public static 人物数据 显示人物 => 人物字典[_配置信息字典.GetValueOrDefault("当前人物","loris")];
    public static List<脚本信息> 配置脚本列表 = [];
    public static Dictionary<string, string> 工具路径字典=new();
    private static Dictionary<string, string> _配置信息字典=new();
    public static Main _单例;
    public static 脚本信息 当前脚本;
    public override void _Ready()
    {
        _单例 = this;
        //TranslationServer.SetLocale("ru");
        var path = ConfigPath;
        var 工具字典 = LoadUtil.FromJson<Dictionary<string, string>>(Path.Combine(path,工具配置文件名));
        if (工具字典!=null)
        {
            foreach (var (k,v) in 工具字典)
            {
                工具路径字典[k] = v;
            }
        }
        _配置信息字典 = LoadUtil.FromJson<Dictionary<string, string>>(Path.Combine(path,配置信息文件名));
        LoadUtil.LoadI18nCSV(Path.Combine(path,本地化文件名));
        IO.单例.setG("bin",BinPath);
        IO.单例.setG("config",ConfigPath);
        IO.单例.setG("mod",ModPath);
        IO.单例.setG("save",GetOutputDir());
        var 初始化列表 = LoadUtil.FromJson<List<初始化信息>>(Path.Combine(path,初始化配置文件名));
        if (初始化列表!=null)
        {
            foreach (var 初始化信息 in 初始化列表)
            {
                OS.ExecuteWithPipe(GetExternalToolPath(初始化信息.tool),初始化信息.arguments);
            }
        }
    }
    public static bool IgnorePath(string path) => Path.GetFileName(path).StartsWith("_");
    public static void 选择脚本(脚本信息 脚本信息)
    {
        当前脚本 = 脚本信息;
        IO.单例.set("tool",GetExternalToolPath(脚本信息.tool));
        IO.单例.set("script",脚本信息.Path);
        IO.单例.set("mod",脚本信息.ModPath);
        IO.单例.set("out",GetOutputDir());
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
    private static void 执行函数完成()
    {
        CharAnim.开始庆祝();
        if (当前脚本.showOut)
        {
            Dialogue.文件处理完成();
        }
        else
        {
            Dialogue.关闭标题();
        }
    }
    private static void 运行预处理函数(脚本信息 脚本信息)
    {
        // 直接调用通用提取函数
        RunScriptTask(脚本信息, 预处理函数名, "预处理函数完成");
    }
    private static void 运行执行函数(脚本信息 脚本信息)
    {
        if (脚本信息.wait)
        {
            Dialogue.显示标题(_单例.Tr("wait")); 
        }

        // 直接调用通用提取函数
        RunScriptTask(脚本信息, 执行函数名, "执行函数完成");
        Dialogue.脚本结束();
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
    public void 处理路径(string[] keys, Dictionary<string, 抬头信息> 索引映射,Dictionary<string, List<索引脚本信息>> 脚本映射,List<索引脚本信息>通用脚本列表,string Ask)
    {
        var extension = Path.GetExtension(keys[0]);
        var ask = Tr(Ask);
        if (keys.Length == 1)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var 询问 = (IndexLoader.文件索引映射.TryGetValue(extension, out var 抬头信息) )? 抬头信息.name : ask;
            显示脚本选择(IndexLoader.文件脚本映射.TryGetValue(extension, out var list) ? list : [], 通用脚本列表, 询问);
        }
        else if (keys.Length > 1)
        {
            // 1. 获取所有不重复的扩展名 (建议统一转小写以防大小写不一致)
            // 使用 HashSet 或 Distinct 过滤重复项
            var distinctExtensions = keys
                .Select(Path.GetExtension)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            // 这里的 extension 变量取的是 distinctExtensions[0] 用于做主要的 Key 查找
            // 因为一个合法的脚本必然挂载在所有涉及的扩展名下，所以查第一个即可
            var keyExtension = distinctExtensions.FirstOrDefault();

            if (keyExtension == null) return; // 防御性编程

            // 2. 判断扩展名是否一致
            if (distinctExtensions.Count == 1)
            {
                // === 场景 A: 扩展名都一样 (例如全是 .jpg) ===
                // 逻辑: 只需要检测 Batch 属性
                    
                if (脚本映射.TryGetValue(keyExtension, out var list))
                {
                    // 筛选出支持 Batch 的脚本
                    var batchScripts = list.Where(s => s.batch).ToList();
                    if (batchScripts.Count+通用脚本列表.Count > 0)
                    {
                        var 对话 = ((索引映射.TryGetValue(keyExtension, out var headerInfo)) && headerInfo.batch)
                            ? headerInfo.name
                            : ask;
                        显示脚本选择(batchScripts,通用脚本列表,对话);
                    }
                }

            }
            else
            {
                // === 场景 B: 扩展名不一样 (例如 .jpg 和 .png 混杂) ===
                // 逻辑: 检测 Batch + Multi + Extensions 列表包含关系

                // 优化思路: 我们不需要遍历所有脚本。
                // 如果一个脚本支持处理这些文件，它一定在 keyExtension (第一个扩展名) 的映射列表中。
                if (脚本映射.TryGetValue(keyExtension, out var list))
                {
                    var validScripts = list.Where(s => 
                        s.batch &&      // 必须支持批处理
                        s.multi &&      // 必须支持混用
                        s.extensions != null && // 安全检查
                        // 关键: 脚本支持的 Extensions 列表必须包含当前拖入的所有扩展名
                        // 即: distinctExtensions 是 s.Extensions 的子集
                        distinctExtensions.All(reqExt => s.extensions.Contains(reqExt))
                    ).ToList();

                    if (validScripts.Count+通用脚本列表.Count > 0)
                    {
                        var 对话 = ((索引映射.TryGetValue(keyExtension, out var headerInfo)) && headerInfo.batch&& headerInfo.multi)
                            ? headerInfo.name
                            : ask;
                        显示脚本选择(validScripts,通用脚本列表,对话);
                    }
                }
            }
        }
        
        // 关键点 4: 强制刷新一次输入状态，防止 PopupMenu 监听不到第一次点击
        Input.FlushBufferedEvents();
    }

    private static void 显示脚本选择(List<索引脚本信息>脚本列表,List<索引脚本信息>通用脚本列表,string 对话)
    {
        脚本列表.AddRange(通用脚本列表);
        Dialogue.显示脚本选项(脚本列表,对话);
    }
    /// <summary>
    /// 通用的脚本异步执行工具函数
    /// </summary>
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
    private static void 预处理函数完成()
    {
        加载对话(Path.Combine(当前脚本.Path,对话文件名));
    }

    private static string GetOutputDir()
    {
        // 1. 获取当前日期并格式化为 "M-d" (例如 1-10)
        // 如果你希望 1月1日显示为 01-01，可以使用 "MM-dd"
        var folderName = DateTime.Now.ToString("MM-dd");

        // 2. 合并完整路径
        var fullPath = Path.Combine(OS.GetUserDataDir(), folderName);

        // 3. 检查目录是否存在，不存在则创建
        // CreateDirectory 方法内部会自动判断，如果目录已存在则不执行任何操作
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }
    public static string GetExternalToolPath(string toolName)
    {
        if (string.IsNullOrEmpty(toolName)) return null;
        if (工具路径字典.TryGetValue(toolName,out var path) && Path.Exists(path))
        {
            return path;
        }
// 1. 先拼接基础路径 (bin/XX)
        var exePath = Path.Combine(BinPath, toolName);

// 2. 处理 Windows 下的扩展名
        if (OS.GetName() == "Windows" && !exePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            exePath += ".exe";
        }

// 3. 逻辑判断：如果基础路径下的文件不存在，则尝试查找子目录下的路径
        if (!File.Exists(exePath))
        {
            // 构造 bin/XX/XX.exe 这种结构
            var subDirName = toolName;
            var fileName = toolName;
    
            if (OS.GetName() == "Windows" && !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".exe";
            }

            var nestedPath = Path.Combine(BinPath, subDirName, fileName);

            // 如果子目录下的文件确实存在，则更新 exePath
            if (File.Exists(nestedPath))
            {
                exePath = nestedPath;
            }
        }

        return ProjectSettings.GlobalizePath(exePath);
    }
    private static string GetProjectPath(string folderName)
    {
        return OS.HasFeature("editor") 
            ? ProjectSettings.GlobalizePath($"res://{folderName}") 
            : Path.Combine(OS.GetExecutablePath().GetBaseDir(), folderName);
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