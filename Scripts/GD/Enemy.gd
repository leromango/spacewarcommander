class_name Enemy extends CharacterBody3D

@onready var bulletPoint : Node3D = $BulletPoint
@onready var beehaviourTree : BeehaveTree = $FirstEnemyBeahviourTree
@export var bulletScene : PackedScene
var movementSpeed : float
@export var startingMovementSpeed : float = 0.05
@export var fleeMovementSpeed : float = 0.9
@export var maxHealth : float = 100
@export var damage : float = 20
@export var escape_threshold_health : float = 60
@export var onDeathScore : int = 5
@export var SpawnRangeMin : float = 100
@export var SpawnRangeMax : float = 400
@export var deathSoundDB : float = 10
@export var shootSoundDB : float = 10
@export var deathSound : AudioStream
@export var shootSound : AudioStream
@export var musicSound : AudioStream
var currentHealth : float
var shouldFlee : bool = true
var canCheckCollision = true
@onready var waitTimeInit : float = $CheckCollisionTimer.wait_time

@export var forwardCheckAvoidance : float = 400


func shoot(playerLocation : Vector3) -> bool:
	look_at(playerLocation)
	var spawnedBullet = bulletScene.instantiate()
	if spawnedBullet == null:
		print_debug("BULLET INVALID")
		return false
	if canCheckCollision:
		canCheckCollision = false
		$CheckCollisionTimer.start()
	
	spawnedBullet.initializeBullet(self as Node3D, $BulletPoint.global_transform, damage)
	get_parent().add_child(spawnedBullet)
	SoundManager.playSound3D(shootSound, global_position, null, shootSoundDB)
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
	
func _physics_process(delta: float) -> void:
	if canCheckCollision:
		if move_and_slide():
			velocity += Vector3(randf_range(-5, 5), 0, 0)
	
func init_enemy(playerPosition : Vector3) -> void:
	global_position = playerPosition + Vector3(1 if randi() % 2 == 0 else -1 * randf_range(SpawnRangeMin, SpawnRangeMax), 1 if randi() % 2 == 0 else -1 *  randf_range(SpawnRangeMin, SpawnRangeMax), 1 if randi() % 2 == 0 else -1 * randf_range(SpawnRangeMin, SpawnRangeMax))
	if global_position.distance_to(playerPosition) <= 200:
		global_position += Vector3(300, 300, 0)
	
func is_at_location(location : Vector3, acceptibleRadius : float = 100) -> bool:
	return global_position.distance_to(location) <= acceptibleRadius
	
func _process(delta: float) -> void:
	pass

func die() -> void:
	# TODO: stuff here
	# LevelPlay.call("enemy_death", onDeathScore)
	SoundManager.playSound3D(deathSound, global_position, null, deathSoundDB)
	get_parent_node_3d().enemy_death(global_position, onDeathScore)
	queue_free()

func reduceHealth(damage: float) -> void:
	currentHealth -= damage
	if (currentHealth <= 0):
		die()
		return
	# print(name + " New Health " + str(currentHealth))
	if shouldFlee and currentHealth <= escape_threshold_health:
		shouldFlee = false
		$HostileEnableTimer.start()
		$Blackboard.set_value("ShouldBeHostile", false)


func _on_hostile_enable_timer_timeout() -> void:
	shouldFlee = true


func _on_check_collision_timer_timeout() -> void:
	canCheckCollision = true
	
