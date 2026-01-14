extends RefCounted

func execute(info: Dictionary) -> void:
	# 获取内容，确保 content 是字符串类型
	var content = info.get("in", [""])[0]
	
	if content == "":
		print("没有内容可以翻译")
		return

	# 对内容进行 URL 编码，防止特殊字符（如空格、#、&）导致 URL 断裂
	var encoded_content = content.uri_encode()
	
	# 构建百度翻译的 URL（这里默认设置为 自动检测 翻译为 英文，你也可以根据需要修改路径）
	var url = "https://fanyi.baidu.com/#auto/en/" + encoded_content
	
	# 调用系统默认浏览器打开
	var err = OS.shell_open(url)
	
	if err != OK:
		print("无法打开浏览器，错误码：", err)