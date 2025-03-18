class_name Enemy extends CharacterBody3D

@onready var bulletPoint : Node3D = $BulletPoint
@onready var beehaviourTree : BeehaveTree = $FirstEnemyBeahviourTree
@export var bulletScene : PackedScene
@export var movementSpeed : float = 20
@export var maxHealth : float = 100
@export var damage : float = 20
@export var escape_threshold_health : float = 20
var currentHealth : float

func shoot(playerLocation : Vector3) -> bool:
	look_at(playerLocation)
	var spawnedBullet = bulletScene.instantiate()
	if spawnedBullet == null:
		print_debug("BULLET INVALID")
		return false
	spawnedBullet.initializeBullet(self as Node3D, $BulletPoint.global_transform, damage)
	get_parent().add_child(spawnedBullet)
	return true

func _ready() -> void:
	currentHealth = maxHealth
	var playerRef = get_parent().get_node_or_null("Player")
	if playerRef == null:
		printerr("PLAYER REF ERROR")
	$Blackboard.set_value("PlayerNode3D", playerRef)

func moveToLocation(location : Vector3) -> void:
	look_at(location)
	set_global_position(global_position + ((location - global_position) * movementSpeed * get_physics_process_delta_time()))
	
func is_alive() -> bool:
	return currentHealth > 0

func is_at_location(location : Vector3, acceptibleRadius : float = 100) -> bool:
	return global_position.distance_to(location) <= acceptibleRadius
	
func _process(delta: float) -> void:
	pass

func die() -> void:
	pass

func got_shot(damage: float) -> void:
	currentHealth -= damage
	if (currentHealth <= 0):
		die()
		return
	if (currentHealth <= escape_threshold_health):
		$Blackboard.set_value("ShouldBeHostile", false)
