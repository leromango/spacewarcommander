extends SerComm

var parentPlayerNode

var currentString : String = ""
# We don't need Process function. The documentation says the message receiving is done in messsage_read
func _ready() -> void:
	parentPlayerNode = get_parent()
	if open_serial():
		on_message.connect(message_read)

func _exit_tree() -> void:
	close_serial()

func send_reload_prompt() -> void:
	# print("reload prompt sent")
	# write_serial("r")
	currentString += "r"

func send_light_indicator(direction: int) -> void:
	# print("indicator: " + str(direction))
	# write_serial("a" + str(direction) + "a")
	currentString += "a" + str(direction) + "a"

func send_buzzer_buzz() -> void:
	# print("buzzer")
	# write_serial("b")
	currentString += "b"

func message_read(m):
	# The documentation says the message receiving is done in messsage_read
	# DON'T do process()
	print(m)
	var elements : PackedStringArray = m.split(",", false)
	if elements.is_empty() or len(elements) < 5:
		return
	var knobValue : float = elements[0].to_float()
	var shootState : int = elements[1].to_int()
	var moveState : int = elements[2].to_int()  # TODO: CHECK moveState
	var switchState : int = elements[3].to_int()
	var changeDirectionState : int = elements[4].to_int()
	var flashlightValue : float = elements[5].to_float()
	var reloadValue : int = elements[6].to_int()
	print(str(knobValue) + "," + str(shootState) + "," + str(moveState) + "," + str(switchState) + "," + str(changeDirectionState) + "," + str(flashlightValue) + "," + str(reloadValue))
	# print(m)
	if shootState == 1:
		shoot()
	if switchState == 1:
		pass
	if changeDirectionState == 1:
		changeAxis()
	if reloadValue == 1:
		reloaded()
	move(moveState)  # TODO: CHECK moveState
	setFlashlightValue(flashlightValue)
	resourceDistribution(knobValue)

func reloaded() -> void:
	parentPlayerNode.Reload()

func shoot() -> void:
	parentPlayerNode.Shoot()

func changeAxis() -> void:
	parentPlayerNode.changeRotationAxis()

func resourceDistribution(dist: float) -> void:
	pass
	# parentPlayerNode.SetDistribution(dist)

func setFlashlightValue(value : float) -> void:
	pass

func move(value : float) -> void:
	parentPlayerNode.move(value)

#func _process(delta: float) -> void:
	#pass
	# write_message()
	#if currentString.is_empty():
		#return
	# print(currentString)
	#write_serial(currentString)
	#currentString = ""

func write_message() -> void:
	# print("Trying...")
	if currentString.is_empty() or not open_serial():
		return
	print(currentString)
	write_serial(currentString)
	currentString = ""
