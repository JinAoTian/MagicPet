using Godot;

namespace desktop.script.Asset;
[GlobalClass]
public partial class IconResource : Resource
{
    [Export] public Texture2D 关机图标;
    [Export] public Texture2D 目录图标;
    [Export] public Texture2D 复制图标;
    [Export] public Texture2D 剪切图标;
    [Export] public Texture2D 取消图标;
    [Export] public Texture2D 返回图标;
}