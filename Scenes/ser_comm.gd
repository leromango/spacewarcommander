extends SerComm


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var player : CharacterBody3D = get_parent() as CharacterBody3D;

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
