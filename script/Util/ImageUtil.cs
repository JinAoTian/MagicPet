using Godot;
using FileAccess = Godot.FileAccess;

namespace desktop.script.Util;

public static class ImageUtil
{
    public static bool Loadimage(string path, out Texture2D image)
    {
        image = null;

        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"[ImageUtil] 路径无效: {path}");
            return false;
        }

        var buffer = FileAccess.GetFileAsBytes(path);
        var img = new Image();
        Error err;
        // 获取后缀名（转小写）
        var extension = path.GetExtension().ToLower();

        // 根据后缀名调用特定的加载函数，这是 4.x 最稳妥的做法
        switch (extension)
        {
            case "png":
                err = img.LoadPngFromBuffer(buffer);
                break;
            case "jpg":
            case "jpeg":
                err = img.LoadJpgFromBuffer(buffer);
                break;
            case "webp":
                err = img.LoadWebpFromBuffer(buffer);
                break;
            case "tga":
                err = img.LoadTgaFromBuffer(buffer);
                break;
            case "bmp":
                err = img.LoadBmpFromBuffer(buffer);
                break;
            case "svg":
                err = img.LoadSvgFromBuffer(buffer);
                break;
            default:
                GD.PrintErr($"[ImageUtil] 不支持的图片格式: {extension}");
                return false;
        }

        if (err != Error.Ok)
        {
            GD.PrintErr($"[ImageUtil] 解析 {extension} 失败: {err}");
            return false;
        }

        // 成功解析后转换为纹理
        image = ImageTexture.CreateFromImage(img);
        return true;
    }
}