class_name Enemy extends CharacterBody3D

@onready var bulletPoint : Node3D = $BulletPoint
@onready var beehaviourTree : BeehaveTree = $FirstEnemyBeahviourTree
@export var bulletScene : PackedScene
var movementSpeed : float
@export var startingMovementSpeed : float = 0.05
@export var maxHealth : float = 100
@export var damage : float = 20
@export var escape_threshold_health : float = 60
var currentHealth : float
var shouldFlee : bool = true

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
	movementSpeed = startingMovementSpeed
	currentHealth = maxHealth
	var playerRef = get_parent().get_node_or_null("Player")
	if playerRef == null:
		printerr("PLAYER REF ERROR")
	$Blackboard.set_value("PlayerNode3D", playerRef)

func moveToLocation(location : Vector3) -> void:
	look_at(location)
	set_global_position(global_position + ((location - global_position).normalized() * movementSpeed))
	
func is_alive() -> bool:
	return currentHealth > 0

func is_at_location(location : Vector3, acceptibleRadius : float = 100) -> bool:
	return global_position.distance_to(location) <= acceptibleRadius
	
func _process(delta: float) -> void:
	pass

func die() -> void:
	# TODO: stuff here
	queue_free()

func reduceHealth(damage: float) -> void:
	currentHealth -= damage
	if (currentHealth <= 0):
		die()
		return
	print(name + " New Health " + str(currentHealth))
	if shouldFlee and currentHealth <= escape_threshold_health:
		shouldFlee = false
		$HostileEnableTimer.start()
		$Blackboard.set_value("ShouldBeHostile", false)


func _on_hostile_enable_timer_timeout() -> void:
	shouldFlee = true
