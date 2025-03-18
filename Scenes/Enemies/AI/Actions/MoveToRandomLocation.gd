extends ActionLeaf

@export var distanceAway : float = 1000
@export var randomnessOfDestinationRange : float = 1000
@export var acceptibleRadius : float = 300

func tick(actor: Node, blackboard: Blackboard) -> int:
	var previousDestination = blackboard.get_value("DestinationLocation")
	var selfRef : Enemy = actor as Enemy
	if (previousDestination == null):
		previousDestination = selfRef.global_position + (selfRef.global_transform.basis.z * distanceAway)
		previousDestination += Vector3(randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange), randf_range(-randomnessOfDestinationRange, randomnessOfDestinationRange))
		blackboard.set_value("DestinationLocation", previousDestination)
		# interrupt(actor, blackboard)
	previousDestination = previousDestination as Vector3
	if selfRef.is_at_location(previousDestination, acceptibleRadius):
		return SUCCESS
	selfRef.moveToLocation(previousDestination)
	return RUNNING
