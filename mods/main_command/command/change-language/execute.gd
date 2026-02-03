extends RefCounted

func execute(info: Dictionary) -> void:
	var lang = info.get("lang", "")
	IO.ChangeLang(lang)