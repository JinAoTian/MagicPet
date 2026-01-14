extends RefCounted

func execute(info: Dictionary) -> void:
	var command =  info.get("exe", "")
	
	var args: PackedStringArray = []
	var pid = OS.execute(command, args, false, false, false)
	if pid == -1:
		print("启动失败！请检查环境变量中是否存在该程序。")
	else:
		print("游戏已启动，PID 为: ", pid)
