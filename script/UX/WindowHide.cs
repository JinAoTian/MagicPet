using System;
using System.Runtime.InteropServices;
using Godot;

namespace desktop;

public partial class WindowHide : Node
{
	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	private const int GwlExstyle = -20;
	private const int WsExToolwindow = 0x00000080;
	private const int WsExAppwindow = 0x00040000;
	private const uint SwpNomove = 0x0002;
	private const uint SwpNosize = 0x0001;
	private const uint SwpNozorder = 0x0004;
	private const uint SwpFramechanged = 0x0020;

	public override void _Ready()
	{

		var hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle);

		if (hWnd != IntPtr.Zero)
		{
			// 1. 获取当前扩展样式
			var exStyle = GetWindowLong(hWnd, GwlExstyle);
            
			// 2. 移除 APPWINDOW（任务栏大图标），添加 TOOLWINDOW（隐藏任务栏）
			exStyle &= ~WsExAppwindow;
			exStyle |= WsExToolwindow;
            
			// 3. 设置新样式
			SetWindowLong(hWnd, GwlExstyle, exStyle);
            
			// 4. 关键：刷新窗口样式以生效
			SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SwpNomove | SwpNosize | SwpNozorder | SwpFramechanged);
		}
	}
}