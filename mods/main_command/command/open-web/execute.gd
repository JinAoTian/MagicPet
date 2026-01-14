extends RefCounted

func execute(info: Dictionary) -> void:
	# 从 IO 中获取名为 "web" 的变量
	var web = info.get("web", "")
	
	# 逻辑处理：确保 web 是一个有效的字符串且不为空
	if web is String and web != "":
		# 调用系统默认浏览器打开网址
		var err = OS.shell_open(web)
		
		if err == OK:
			print("成功发起打开网址请求: ", web)
		else:
			push_error("无法打开网址，错误代码: " + str(err))
	else:
		push_error("无效的网址地址: " + str(web))
