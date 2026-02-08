using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using desktop.script.Loader;
using desktop.script.logic;
using Godot;
using Newtonsoft.Json;
using Environment = System.Environment;
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

    public static void WriteJson(string path, object obj)
    {
        // 1. 确保目标文件夹存在，否则会抛出 DirectoryNotFoundException
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 2. 配置序列化设置
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented, // 产生带缩进的 JSON，易于人类阅读
            DateFormatString = "yyyy-MM-dd HH:mm:ss" // 可选：统一时间格式
        };

        // 3. 将对象转换为 JSON 字符串
        var jsonContent = JsonConvert.SerializeObject(obj, settings);

        // 4. 写入文件（使用 UTF-8 编码以支持中文）
        File.WriteAllText(path, jsonContent, Encoding.UTF8);
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
        // 基础参数校验：空值/空白字符直接返回空
        if (string.IsNullOrWhiteSpace(toolName))
        {
            GD.PrintErr("[工具地址查询] 工具名称不能为空！");
            return null;
        }

        // 1. 优先从自定义字典获取配置值（支持路径、网址、任意字符串）
        var configValue = Main.工具路径字典.GetValueOrDefault(toolName);
        if (!string.IsNullOrEmpty(configValue))
        {
            GD.Print($"[工具地址查询] 从配置字典获取：{toolName} -> {configValue}");
            return configValue;
        }

        // 2. 字典无配置，尝试从Windows系统PATH环境变量查找本地可执行文件
        try
        {
            var systemPath = FindToolInSystemPath(toolName);
            if (!string.IsNullOrEmpty(systemPath))
            {
                GD.Print($"[工具地址查询] 从系统环境变量获取：{toolName} -> {systemPath}");
                return systemPath;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[工具地址查询] 检索系统环境变量异常：{ex.Message}");
        }

        // 3. 所有渠道均未找到
        GD.PrintErr($"[工具地址查询] 未找到匹配项：{toolName}");
        return null;
    }
    private static string FindToolInSystemPath(string toolName)
    {
        // Windows可执行文件后缀，仅用于检索本地程序
        string[] executableExtensions = [".exe"];
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var pathDirs = pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var dir in pathDirs)
        {
            // 跳过不存在的目录
            if (!Directory.Exists(dir)) 
                continue;

            foreach (var ext in executableExtensions)
            {
                var fullPath = Path.Combine(dir, $"{toolName}{ext}");
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }
    public static void 初始化()
    {
        var path = ConfigPath;
        IO.单例.setG("config",path);
        IO.单例.setG("mod",ModPath);
        IO.单例.setG("save",GetOutputDir());
        LoadI18nCSV(Path.Combine(path,Main.本地化文件名));
        ConfigLoader.加载配置信息(path);
    }
}