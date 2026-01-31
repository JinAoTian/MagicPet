using System;
using System.IO;
using Steamworks.Ugc;
using System.Threading.Tasks;
using desktop.script.Util;
using Godot;
// ReSharper disable UnassignedField.Global

// ReSharper disable InconsistentNaming
namespace desktop.script.Steam;

public static class WorkShop
{
    public static async Task<bool> PublishItem(string path)
    {
        // 检查 Steam 状态
        if (!Steamworks.SteamClient.IsValid)
        {
            GD.PrintErr("Steam 客户端未初始化，请先启动 Steam！");
            return false;
        }
        var infoPath = Path.Combine(path, "info.json");
        var info = LoadUtil.FromJson<ModInfo>(infoPath);
        if (info == null) return false;
        var icon = Path.Combine(path, "icon.png");
        if (!File.Exists(icon))
        {
            icon = Path.Combine(path, "icon.jpg");
            if (!File.Exists(icon)) return false;
        }
        // 1. 初始化 Editor
        var isNewItem = info.itemid == 0;
        var editor = isNewItem ? Editor.NewCommunityFile : new Editor(info.itemid);
        
        // 2. 配置物品元数据
        // 强制更新的内容：文件夹路径和更新日志
        editor = editor.WithContent(path)
            .WithChangeLog(info.changenote ?? "");

        // 仅在“新建物品”时上传的部分，避免覆盖云端已更改的标题、描述和缩略图
        if (isNewItem)
        {
            editor = editor.WithTitle(info.title ?? "New Mod")
                .WithDescription(info.description ?? "")
                .WithPreviewFile(icon);
        }

        // 设置可见性 (通常建议更新时也同步可见性，若想完全由云端控制，可将此部分也放入 isNewItem 判断中)
        editor = info.visibility switch
        {
            0 => editor.WithPublicVisibility(),
            1 => editor.WithFriendsOnlyVisibility(),
            _ => editor.WithPrivateVisibility()
        };

        // 添加标签
        if (info.tags != null)
        {
            foreach (var tag in info.tags)
            {
                editor = editor.WithTag(tag);
            }
        }

        // 3. 提交上传
        GD.Print("开始上传到 Steam 创意工坊...");
        var result = await editor.SubmitAsync();

        // 4. 处理结果
        if (result.Success)
        {
            GD.Print($"上传成功! 物品 ID: {result.FileId}");
    
            // 检查是否有必要更新本地 ID
            if (info.itemid != result.FileId.Value)
            {
                info.itemid = result.FileId.Value;
                // 使用 try-catch 保护文件写入，防止因文件占用导致上传成功但 ID 丢失
                try 
                {
                    LoadUtil.WriteJson(infoPath, info);
                }
                catch (Exception e)
                {
                    GD.PrintErr($"ID 写入失败，请手动记录: {info.itemid}. 错误: {e.Message}");
                }
            }
            return true;
        }
        GD.PrintErr($"上传失败: {result.Result}");
        return false;
    }
}

[Serializable]
// ReSharper disable once ClassNeverInstantiated.Global
public class ModInfo
{
    public ulong itemid;//对应publishedfileid
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public uint visibility=2;
    public string title;
    public string description;
    public string changenote;
    public string[] tags;
}