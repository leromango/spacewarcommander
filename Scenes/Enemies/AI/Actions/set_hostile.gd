extends ActionLeaf

func tick(actor: Node, blackboard: Blackboard) -> int:
	blackboard.set_value("ShouldBeHostile", true)
	return SUCCESS
