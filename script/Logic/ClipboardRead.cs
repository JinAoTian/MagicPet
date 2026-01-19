using Godot;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using desktop.script.Loader;
using desktop.script.logic;
using desktop.script.UX;

namespace desktop.script.Logic;

public partial class ClipboardRead : Node
{
    private bool _isMouseInWindow = true;
    

    public override void _UnhandledInput(InputEvent @event)
    {
        // 2. 检查是否按下了 Ctrl (Command)
        var isCtrlPressed = @event is InputEventWithModifiers { CtrlPressed: true };
        
        // 如果是 macOS，通常使用 Command 键，可以用 CommandOrControlAutoremap
        // bool isCtrlPressed = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Meta);

        if (isCtrlPressed && @event is InputEventKey { Pressed: true, Keycode: Key.V })
        {
            var clipboardContent = DisplayServer.ClipboardGet();
            if (!string.IsNullOrEmpty(clipboardContent))
            {
                ProcessClip(clipboardContent);
                return;
            }
            var flies = GetClipboardFilesWin32();
            if (flies!=null)
            {
                FileDrop.DropFiles(flies);
            }
        }
    }

    private void ProcessClip(string content)
    {
        var urls = content.Split("\n");
        var input = new List<string>();
        var hosts = new List<string>();
        var pathAndQuerys = new List<string>();
        foreach (var url in urls)
        {
            if (GetUrl(url,out var host,out var path))
            {
                input.Add(url);
                hosts.Add(host);
                pathAndQuerys.Add(path);
            }
        }

        if (input.Count == 0)
        {
            IO.单例.set("in",new[]{content});
            Dialogue.显示脚本选项(CommandLoader.文本指令列表,Tr("txt-ask"));
            return;
        }
        IO.单例.set("in",input.ToArray());
        IO.单例.set("host",hosts.ToArray());
        IO.单例.set("path",pathAndQuerys.ToArray());
        IO.单例.set("ext",hosts[0]);
        IO.单例.set("single",input.Count == 1);
        GD.Print(urls[0]);
        IndexMatch.处理路径(hosts.ToArray(),IndexLoader.网址索引映射,IndexLoader.网址脚本映射,IndexLoader.网址通用脚本列表,"web-ask");
    }
    
    private static bool GetUrl(string url, out string host, out string path)
    {
        // 初始化输出变量
        host = string.Empty;
        path = string.Empty;

        // 1. 尝试解析字符串为绝对 URI
        // UriKind.Absolute 确保字符串包含协议头（如 http:// 或 https://）
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
        {
            // 2. 验证协议是否为常见的 Web 协议（可选，根据需求增加）
            if (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
            {
                host = uriResult.Host;
            
                // 3. 提取主机名之后的完整路径（包含 Query 参数）
                // PathAndQuery 会返回类似 "/index.html?id=1" 的内容
                path = uriResult.PathAndQuery;
            
                return true;
            }
        }
        return false;
    }

    #region 文件路径获取
    [DllImport("user32.dll")]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);
    [DllImport("user32.dll")]
    static extern bool CloseClipboard();
    [DllImport("user32.dll")]
    static extern IntPtr GetClipboardData(uint uFormat);
    [DllImport("shell32.dll")]
    static extern uint DragQueryFile(IntPtr hDrop, uint iFile, System.Text.StringBuilder lpszFile, uint cch);
    private const uint CfHdrop = 15;
    private string[] GetClipboardFilesWin32()
    {
        var files = new List<string>();
        if (!OpenClipboard(IntPtr.Zero)) return files.ToArray();

        try
        {
            var hDrop = GetClipboardData(CfHdrop);
            if (hDrop != IntPtr.Zero)
            {
                var count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);
                for (uint i = 0; i < count; i++)
                {
                    var sb = new System.Text.StringBuilder(260);
                    DragQueryFile(hDrop, i, sb, (uint)sb.Capacity);
                    files.Add(sb.ToString());
                }
            }
        }
        finally
        {
            CloseClipboard();
        }
        return files.ToArray();
    }
    #endregion
}