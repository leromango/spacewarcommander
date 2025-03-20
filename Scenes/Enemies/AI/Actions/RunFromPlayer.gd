extends ActionLeaf

@export var distanceAway : float = 1000
@export var randomnessOfDestinationRange : float = 1000
@export var acceptibleRadius : float = 300
@export var fleeSpeed : float = 1

func tick(actor: Node, blackboard: Blackboard) -> int:
	var previousDestination = blackboard.get_value("DestinationLocation")
	var selfRef : Enemy = actor as Enemy
	if (previousDestination == null):
		var playerRef : Node3D = blackboard.get_value("PlayerNode3D") as Node3D
		var playerLocation : Vector3 = playerRef.global_position as Vector3
		previousDestination= playerLocation - (playerRef.global_transform.basis.y.normalized() * distanceAway)
		previousDestination += Vector3(randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange))
		blackboard.set_value("DestinationLocation", previousDestination)
		# interrupt(actor, blackboard)
	previousDestination = previousDestination as Vector3
	if selfRef.is_at_location(previousDestination, acceptibleRadius):
		blackboard.set_value("DestinationLocation", null)
		return SUCCESS
	selfRef.movementSpeed = fleeSpeed
	selfRef.moveToLocation(previousDestination)
	return RUNNING
