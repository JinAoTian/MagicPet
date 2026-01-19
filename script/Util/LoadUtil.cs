using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using desktop.script.logic;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace desktop.script.Util;

public partial class LoadUtil : Node
{
    /// <summary>
    /// 从指定路径读取 JSON 文件并映射到泛型对象
    /// </summary>
    /// <param name="path">Godot 路径 (如 "res://data.json" 或 "user://config.json")</param>
    public static T FromJson<T>(string path)
    {
        // 1. 检查文件是否存在
        if (!FileAccess.FileExists(path))
        {
            //GD.PrintErr($"[LoadUtil] 找不到文件: {path}");
            return default;
        }

        // 2. 打开并读取文件内容
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[LoadUtil] 无法打开文件: {FileAccess.GetOpenError()}");
            return default;
        }
        var jsonContent = file.GetAsText();
        try
        {
            // 3. 使用 Newtonsoft.Json 进行转换
            var result = JsonConvert.DeserializeObject<T>(jsonContent);
            return result;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[LoadUtil] JSON 解析失败: {ex.Message}");
            return default;
        }
    }
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public static void LoadI18nCSV(string path)
    {
        if (!File.Exists(path))
        {
            //GD.PrintErr($"[I18n] CSV文件不存在: {path}");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // 忽略缺失字段
            HeaderValidated = null,   // 忽略表头验证
        };

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);

        // 1. 读取表头并识别语言
        csv.Read();
        csv.ReadHeader();
        var headerLabels = csv.HeaderRecord; 
        // 假设 header 为: [id, zh_CN, en, ja...]

        var translations = new Dictionary<string, Translation>();

        // 为每个语言列创建一个 Translation 实例
        for (var i = 1; i < headerLabels.Length; i++)
        {
            var locale = headerLabels[i].Trim();
            translations[locale] = new Translation { Locale = locale };
        }

        // 2. 逐行读取数据
        while (csv.Read())
        {
            // 获取第一列作为 Key
            var key = csv.GetField(0);
            if (string.IsNullOrWhiteSpace(key)) continue;

            // 遍历该行的后续列
            for (var i = 1; i < headerLabels.Length; i++)
            {
                var locale = headerLabels[i];
                var value = csv.GetField(i);

                // 处理转义换行符并存入对应的语言包
                if (translations.ContainsKey(locale))
                {
                    translations[locale].AddMessage(key, value.Replace("\\n", "\n"));
                }
            }
        }

        // 3. 注册到 Godot TranslationServer
        foreach (var trans in translations.Values)
        {
            TranslationServer.AddTranslation(trans);
            //GD.Print($"[I18n] 动态载入成功: {trans.Locale} (已注入 {trans.GetMessageList().Length} 条文本)");
        }
        // 4. 强制刷新当前语言显示
        TranslationServer.SetLocale(TranslationServer.GetLocale());
    }

    private static string BinPath => GetProjectPath("bin");
    private static string ConfigPath => GetProjectPath("config");
    public static string ModPath => GetProjectPath("mods");
    private static string GetProjectPath(string folderName)
    {
        return OS.HasFeature("editor") 
            ? ProjectSettings.GlobalizePath($"res://{folderName}") 
            : Path.Combine(OS.GetExecutablePath().GetBaseDir(), folderName);
    }
    public static string GetOutputDir()
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
        if (Main.工具路径字典.TryGetValue(toolName,out var path) && Path.Exists(path))
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
    public static void 初始化()
    {
        var path = ConfigPath;
        IO.单例.setG("bin",BinPath);
        IO.单例.setG("config",path);
        IO.单例.setG("mod",ModPath);
        IO.单例.setG("save",GetOutputDir());
        LoadI18nCSV(Path.Combine(path,Main.本地化文件名));
        var 初始化列表 = FromJson<List<初始化信息>>(Path.Combine(path,Main.初始化配置文件名));
        if (初始化列表!=null)
        {
            foreach (var 初始化信息 in 初始化列表)
            {
                OS.ExecuteWithPipe(GetExternalToolPath(初始化信息.tool),初始化信息.arguments);
            }
        }
        var 工具字典 = FromJson<Dictionary<string, string>>(Path.Combine(path,Main.工具配置文件名));
        if (工具字典!=null)
        {
            foreach (var (k,v) in 工具字典)
            {
                Main.工具路径字典[k] = v;
            }
        }
        Main._配置信息字典 = FromJson<Dictionary<string, string>>(Path.Combine(path,Main.配置信息文件名));
    }
}