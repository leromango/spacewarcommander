class_name Enemy extends CharacterBody3D

@onready var bulletPoint : Node3D = $BulletPoint
@onready var beehaviourTree : BeehaveTree = $FirstEnemyBeahviourTree
@export var bulletScene : PackedScene
@export var movementSpeed : float = 20

func shoot(playerLocation : Vector3) -> bool:
	var spawnedBullet = bulletScene.instantiate() as Node3D
	if spawnedBullet == null:
		printerr("BULLET SCENE INVALID")
		return false
	spawnedBullet.set_global_position(playerLocation)
	get_parent().add_child(spawnedBullet)
	return true

func _ready() -> void:
	var playerRef = get_parent().get_node_or_null("Player")
	if playerRef == null:
		printerr("PLAYER REF ERROR")
	$Blackboard.set_value("PlayerNode3D", playerRef)

func moveToLocation(location : Vector3) -> void:
	look_at(location)
	set_global_position(global_position + ((location - global_position) * movementSpeed))

func is_at_location(location : Vector3, acceptibleRadius : float = 100) -> bool:
	return global_position.distance_to(location) <= acceptibleRadius
	
func _process(delta: float) -> void:
	pass
