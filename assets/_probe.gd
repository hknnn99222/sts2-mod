extends SceneTree
func _init():
    ProjectSettings.load_resource_pack("D:/SteamLibrary/steamapps/workshop/content/2868840/3737335127/BaseLib/BaseLib.pck", true)
    var paths = [
      "res://BaseLib/images/powers/baselib-power_temp_up.png",
      "res://BaseLib/images/powers/big/baselib-power_temp_up.png",
      "res://BaseLib/images/powers/big/baselib-power_temp_up_big.png",
      "res://images/powers/baselib-power_temp_up.png",
    ]
    for p in paths:
        var t = load(p)
        if t == null: print("MISS ", p)
        else: print("OK   ", p, " -> ", t.get_width(), "x", t.get_height())
    var root = DirAccess.open("res://")
    if root: print("root entries: ", root.get_directories())
    quit()
