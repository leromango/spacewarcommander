extends ActionLeaf


func tick(actor: Node, blackboard: Blackboard) -> int:
	# super(actor, blackboard)
	var playerRef = blackboard.get_value("PlayerNode3D")
	var selfRef : Enemy = actor as Enemy
	var playerLocation : Vector3 = playerRef.global_position as Vector3
	var bresult : bool = selfRef.shoot(playerLocation)
	return SUCCESS if bresult else FAILURE
