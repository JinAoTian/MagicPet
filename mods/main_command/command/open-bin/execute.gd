extends RefCounted

func execute(info: Dictionary) -> void:
	var path = IO.getG("bin")
	if DirAccess.dir_exists_absolute(path):
		OS.shell_open(path)
	else:
		print("错误：路径不存在 -> ", path)
