extends ActionLeaf

@export var distanceAway : float = 1000
@export var randomnessOfDestinationRange : float = 1000
@export var acceptibleRadius : float = 300
@export var maxDistanceFromPlayer : float = 1000

func tick(actor: Node, blackboard: Blackboard) -> int:
	if blackboard.get_value("ShouldBeHostile") == false:
		blackboard.set_value("DestinationLocation", null)
		return FAILURE
	var previousDestination = blackboard.get_value("DestinationLocation")
	var selfRef : Enemy = actor as Enemy
	if (previousDestination == null):
		var playerRef : Node3D = blackboard.get_value("PlayerNode3D") as Node3D
		previousDestination = selfRef.global_position + (selfRef.global_transform.basis.z.normalized() * distanceAway)
		previousDestination += Vector3(randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange))
		var distance_to_player : float = previousDestination.distance_to(playerRef.global_position)
		# print("distance: " + str(distance_to_player))
		if distance_to_player >= maxDistanceFromPlayer:
			previousDestination = (playerRef.global_position - previousDestination).normalized() * (distance_to_player - maxDistanceFromPlayer)
		blackboard.set_value("DestinationLocation", previousDestination)
		# interrupt(actor, blackboard)
	previousDestination = previousDestination as Vector3
	if selfRef.is_at_location(previousDestination, acceptibleRadius):
		blackboard.set_value("DestinationLocation", null)
		return SUCCESS
	selfRef.moveToLocation(previousDestination)
	return RUNNING
