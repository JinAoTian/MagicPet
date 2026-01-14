using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;

namespace desktop.script.logic;

// ReSharper disable once InconsistentNaming
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class IO : Node
{
    public Dictionary Info = new ();//单例方便dialogue获取
    public Dictionary Global  = new();
    public static IO 单例;
    public override void _Ready()
    {
        单例 = this;
    }
    public void set(string key, Variant value)
    {
        if(Info==null)return;
        Info[key] = value;
    }
    public Variant get(string key)
    {
        if (Info != null && Info.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }    
    public void setG(string key, Variant value)
    {
        if(Global==null)return;
        Global[key] = value;
    }
    public Variant getG(string key)
    {
        if (Global != null && Global.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }
}