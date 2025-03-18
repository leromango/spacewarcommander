extends ConditionLeaf

func tick(actor: Node, blackboard: Blackboard) -> int:
	var selfRef : Enemy = actor as Enemy
	if (selfRef.currentHealth <= selfRef.escape_threshold_health):
		if (blackboard.get_value("ShouldBeHostile") == false):
			return FAILURE
	return SUCCESS
