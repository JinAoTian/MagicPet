using System.IO;
using desktop.script.logic;
using Godot;

namespace desktop.script.Loader;

public partial class NodeLoader : Node
{
    private const string 目录名 = "node";
    private const string 脚本文件名 = "node.gd";
    public static void 加载场景列表(string 路径)
    {
        var 总目录 = Path.Combine(路径, 目录名);
        if (!Directory.Exists(总目录)) return;
        foreach (var 脚本目录 in Directory.GetDirectories(总目录))
        {
            if(Main.IgnorePath(脚本目录))continue;
            var 脚本文件路径 = Path.Combine(脚本目录, 脚本文件名);
            {
                if (File.Exists(脚本文件路径))
                {
                    LoadNode(脚本文件路径);
                }
            }
        }
    }
    private static void LoadNode(string path)
    {
        var script = GD.Load<GDScript>(path);
        if (script == null) return;

        var newNode = new Node();
        newNode.Name = "ExternalScriptNode";
        newNode.SetScript(script);

        var tree = (SceneTree)Engine.GetMainLoop();
    
        // 关键改动：不要直接 AddChild，而是使用 CallDeferred
        // 这会将“添加节点”的操作排队到当前帧处理完毕后再执行
        tree.Root.CallDeferred(Node.MethodName.AddChild, newNode);

        GD.Print("节点已加入队列，将在下一刻安全添加到根目录");
    }
}