extends RefCounted

func execute(info: Dictionary) -> void:
    var path = info.get("dir", [])[0]
    IO.PublishItem(path)