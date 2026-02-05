extends RefCounted

func execute(info: Dictionary) -> void:
	# 获取内容，确保 content 是字符串类型
	var content = info.get("in", [""])[0]
	
	if content == "":
		print("搜索内容为空")
		return
	
	# 1. 对搜索内容进行 URL 编码，处理空格和特殊字符
	var encoded_content = content.uri_encode()
	
	# 2. 拼接百度搜索的完整 URL
	var url = "https://www.baidu.com/s?wd=" + encoded_content
	
	# 3. 调用系统默认浏览器打开该链接
	var err = OS.shell_open(url)
	
	# 检查是否成功调用
	if err != OK:
		print("无法打开浏览器，错误码：", err)