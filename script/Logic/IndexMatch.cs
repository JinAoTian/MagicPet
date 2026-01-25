using System.Collections.Generic;
using System.IO;
using System.Linq;
using desktop.script.Loader;
using desktop.script.logic;
using desktop.script.UX;
using Godot;

namespace desktop.script.Logic;

public static class IndexMatch
{
    private static List<可见脚本信息> _额外脚本列表;

    public static void 设置额外脚本列表(List<可见脚本信息> 额外脚本)
    {
        _额外脚本列表 = 额外脚本;
    }
    // ReSharper disable once InconsistentNaming
    public static void 处理路径(string[] keys, Dictionary<string, 抬头信息> 索引映射,Dictionary<string, List<索引脚本信息>> 脚本映射,List<索引脚本信息>通用脚本列表,string ask)
    {
        var extension = Path.GetExtension(keys[0]);
        var 脚本执行 = false;
        if (keys.Length == 1)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var 询问 = (IndexLoader.文件索引映射.TryGetValue(extension, out var 抬头信息) )? 抬头信息.name : ask;
            显示脚本选择(IndexLoader.文件脚本映射.TryGetValue(extension, out var list) ? list : [], 通用脚本列表, 询问);
            脚本执行 = true;
        }
        if (keys.Length > 1)
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
                    var 对话 = ((索引映射.TryGetValue(keyExtension, out var headerInfo)) && headerInfo.batch)
                        ? headerInfo.name
                        : ask;
                    显示脚本选择(batchScripts,通用脚本列表,对话);
                    脚本执行 = true;
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
                    var 对话 = ((索引映射.TryGetValue(keyExtension, out var headerInfo)) && headerInfo.batch&& headerInfo.multi)
                        ? headerInfo.name
                        : ask;
                    显示脚本选择(validScripts,通用脚本列表,对话);
                    脚本执行 = true;
                }
            }
        }

        if (!脚本执行)
        {
            显示脚本选择([],通用脚本列表,ask);
        }

        Input.FlushBufferedEvents();
    }
    private static void 显示脚本选择(List<索引脚本信息>脚本列表,List<索引脚本信息>通用脚本列表,string 对话)
    {
        脚本列表.AddRange(通用脚本列表);
        if (_额外脚本列表!=null)
        {
            _额外脚本列表.AddRange(脚本列表);
            Dialogue.显示脚本选项(_额外脚本列表,对话);
            _额外脚本列表 = null;
        }
        else
        {
            Dialogue.显示脚本选项(脚本列表,对话);
        }
    }
}