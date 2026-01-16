extends RefCounted

func execute(info: Dictionary) -> void:
	var copyq_path = info.get("tool", "")
	var files = info.get("in", []) # 假设输入是数组，如果不是请先转换
	
	if typeof(files) != TYPE_ARRAY or files.is_empty():
		return

	# 1. 核心格式转换：将路径转换为 RFC 3986 标准的 URI 列表
	var uris = []
	for f in files:
		var abs_path = ProjectSettings.globalize_path(f).replace("\\", "/")
		# Windows 绝对路径通常以 C:/ 开头，需要补全为 file:///
		var uri = ""
		if abs_path.begins_with("/"):
			uri = "file://" + abs_path
		else:
			uri = "file:///" + abs_path
		uris.append(uri)
	
	var uri_data = "\n".join(uris)

	# 2. 编写 CopyQ 脚本 (使用 ` 作为字符串边界以防路径中有引号)
	var script = "copy('text/uri-list', `%s`);" % uri_data
	
	# 3. 执行 eval 命令
	# OS.execute 的参数要求：[执行文件路径, 参数数组, 输出数组, 是否阻塞, 是否显示窗口]
	var args = ["eval", script]
	var output = []
	OS.execute(copyq_path, args, output)