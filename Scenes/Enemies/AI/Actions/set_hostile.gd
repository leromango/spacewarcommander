extends ActionLeaf

func tick(actor: Node, blackboard: Blackboard) -> int:
	blackboard.set_value("ShouldBeHostile", true)
	var selfRef : Enemy = actor as Enemy
	selfRef.movementSpeed = selfRef.startingMovementSpeed
	return SUCCESS
