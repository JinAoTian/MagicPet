extends RefCounted

func execute(_info: Dictionary) -> void:
	var platform = OS.get_name()
	
	match platform:
		"Windows":
			# Windows: 使用 taskmgr 命令
			OS.execute("cmd.exe", ["/c", "start", "taskmgr"])
		"macOS":
			# macOS: 活动监视器 (Activity Monitor)
			OS.execute("open", ["-a", "Activity Monitor"])
		"Linux":
			# Linux: 不同的桌面环境工具不同，这里以常见的 gnome-system-monitor 为例
			# 或者尝试通用终端指令，但 Linux 差异较大
			OS.execute("gnome-system-monitor", [])
		_:
			print("当前平台暂不支持直接打开任务管理器")