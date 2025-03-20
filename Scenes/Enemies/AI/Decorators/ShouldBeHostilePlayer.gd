extends ConditionLeaf

func tick(actor: Node, blackboard: Blackboard) -> int:
	if (blackboard.get_value("ShouldBeHostile") == false):
		blackboard.set_value("DestinationLocation", null)
		return FAILURE
	return SUCCESS
