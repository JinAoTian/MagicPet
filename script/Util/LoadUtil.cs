using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
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
}