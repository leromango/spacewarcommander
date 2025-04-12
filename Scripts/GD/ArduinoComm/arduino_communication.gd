extends Node

var parentPlayerNode
var socket: WebSocketPeer
var currentString : String = ""
var f: SpotLight3D
@export var flashlightThreshold : float = 950  # For activation for flashlight
@export var flashlightErrorValue : float = 30  # for sudden jumps
var previous_flashlight_value : float = 0

func _ready() -> void:
	parentPlayerNode = get_parent()
	# SoundManager.playSound(parentPlayerNode.get_parent_node_3d().battle_music)
	# Create a new WebSocketPeer instance
	socket = WebSocketPeer.new()
	
	# Connect to WebSocket server
	var error = socket.connect_to_url("ws://localhost:65102")
	if error != OK:
		print("Failed to connect to WebSocket server: ", error)
	else:
		print("Attempting to connect to WebSocket server...")
	f = parentPlayerNode.get_node("Flashlight")
		
func _process(delta: float) -> void:
	if not parentPlayerNode.isAlive():
		return
	socket.poll()
	
	# Check connection state.
	var state = socket.get_ready_state()
	if state == WebSocketPeer.STATE_OPEN:
		while socket.get_available_packet_count() > 0:
			var packet = socket.get_packet()
			if socket.was_string_packet():
				var msg = packet.get_string_from_utf8()
				print("Received message: ", msg)
				message_read(msg)
			else:
				print("Received binary packet: ", packet)
	elif state == WebSocketPeer.STATE_CLOSING:
		# Socket is closing.
		pass
	elif state == WebSocketPeer.STATE_CLOSED:
		var code = socket.get_close_code()
		var reason = socket.get_close_reason()
		print("WebSocket closed with code: %d, reason: %s. Clean: %s" % [code, reason, code != -1])
		set_process(false)

# Expected message format (comma-separated):
# knob,shoot,move,flashlight,reload,sliderX,sliderY
func message_read(m: String) -> void:
	print("Processing message: ", m)
	var elements = m.split(",", false)
	if elements.size() < 7:
		return   # Not enough data
	
	# Parse values (inverted: 0 means active)
	var knobValue: float = elements[0].to_float()           # Resource distribution input
	var shootState: int = elements[1].to_int()                # 0 = shoot
	var moveState: int = elements[2].to_int()                 # 0 = move (now triggers reload)
	var flashlightValue: float = elements[3].to_float()       # Flashlight sensor value
	var reloadValue: int = elements[4].to_int()               # 0 = reload (now triggers rotation update)
	var sliderXValue: int = elements[5].to_int()              # Horizontal slider input (0 to 1024)
	var sliderYValue: int = elements[6].to_int()              # Vertical slider input (0 to 1024)
	
	print("Parsed values: knob=%f, shoot=%d, move=%d, flashlight=%f, reload=%d, sliderX=%d, sliderY=%d" %
		[knobValue, shootState, moveState, flashlightValue, reloadValue, sliderXValue, sliderYValue])
	
	# Process commands based on inverted logic (0 = active). Because Im using pullup in Arduino.
	if shootState == 0:
		# SoundManager.playSound(parentPlayerNode.shootSound)
		shoot()
	# Swapped actions: if moveState is active (0), call reload.
	if moveState == 0:
		reloaded()
	# If reloadValue is active (0), update slider values and update rotation.
	if reloadValue == 0:
		parentPlayerNode.SliderXValue = sliderXValue
		parentPlayerNode.SliderYValue = sliderYValue
		parentPlayerNode.AddRotationFromSliders()  # This adds incremental rotation
	
	# Adjust additional parameters.
	resourceDistribution(knobValue)
	setFlashlightValue(flashlightValue)
	write_message()

# --- Stub functions that call the Player's methods ---
func shoot() -> void:
	parentPlayerNode.Shoot()

func reloaded() -> void:
	parentPlayerNode.Reload()

func resourceDistribution(dist: float) -> void:
	# Adjust distribution, mapping knob (0–1024) to a value (switch divisor to make it slower or idk)
	parentPlayerNode.SetDistribution(dist / 1000)

func setFlashlightValue(value: float) -> void:
	if absf(value - previous_flashlight_value) > flashlightErrorValue:
		previous_flashlight_value = value
		return
	parentPlayerNode.toggleFlashlight(value > flashlightThreshold)
	previous_flashlight_value = value

# --- Functions for sending commands over WebSocket ---
func send_reload_prompt() -> void:
	currentString += "r"

func send_light_indicator(direction: int) -> void:
	currentString += "a" + str(direction) + "a"

func send_buzzer_buzz(turnOffAfterTime: float) -> void:
	currentString += "b"

func write_message() -> void:
	if currentString != "":
		socket.send_text(currentString)
		print(currentString)
		currentString = ""
