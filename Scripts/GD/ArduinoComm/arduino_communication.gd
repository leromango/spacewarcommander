extends Node

# Reference to the parent node that holds game logic
var parentPlayerNode
# Our WebSocket connection object
var socket: WebSocketPeer
var currentString : String = ""


func _ready() -> void:
	# Get the parent node (which might contain functions like Shoot(), Reload(), etc.)
	parentPlayerNode = get_parent()
	
	# Create a new WebSocketPeer instance
	socket = WebSocketPeer.new()
	
	# Connect to your WebSocket server.
	# Replace "ws://localhost:65101" with your actual WebSocket URL.
	var error = socket.connect_to_url("ws://localhost:65102")
	if error != OK:
		print("Failed to connect to WebSocket server: ", error)
	else:
		print("Attempting to connect to WebSocket server...")
		
	# (Optional) Configure handshake headers, heartbeat interval, or other properties here if needed.

func _process(delta: float) -> void:
	# Regularly poll the socket to update its state and retrieve any incoming packets.
	socket.poll()
	
	# Check the connection state.
	var state = socket.get_ready_state()
	if state == WebSocketPeer.STATE_OPEN:
		# Process all available packets.
		while socket.get_available_packet_count() > 0:
			var packet = socket.get_packet()
			# Check if the received packet is a text frame.
			if socket.was_string_packet():
				# Convert the packet (PackedByteArray) to a UTF-8 string.
				var msg = packet.get_string_from_utf8()
				print("Received message: ", msg)
				# Process the received message using your custom logic.
				message_read(msg)
			else:
				# If binary data is received, handle it as needed.
				print("Received binary packet: ", packet)
	elif state == WebSocketPeer.STATE_CLOSING:
		# The socket is in the process of closing.
		pass
	elif state == WebSocketPeer.STATE_CLOSED:
		# The socket is closed; retrieve and log close details.
		var code = socket.get_close_code()
		var reason = socket.get_close_reason()
		print("WebSocket closed with code: %d, reason: %s. Clean: %s" % [code, reason, code != -1])
		# Optionally, stop processing further updates.
		set_process(false)

# This function processes incoming messages (assumed to be comma-separated values)
func message_read(m: String) -> void:
	print("Processing message: ", m)
	# Split the incoming string by commas.
	var elements = m.split(",", false)
	if elements.size() < 7:
		# Not enough data – ignore or handle error.
		return
	
	# Parse sensor and button values.
	var knobValue : float = elements[0].to_float()
	var shootState : int = elements[1].to_int()          # For example, 1 means "shoot"
	var moveState : int = elements[2].to_int()             # For example, 1 means "move"
	var changeDirectionState : int = elements[3].to_int()  # For example, 1 means "change direction"
	var switchState : int = elements[4].to_int()           # Additional state for move function
	var flashlightValue : float = elements[5].to_float()
	var reloadValue : int = elements[6].to_int()           # 1 means "reload"
	
	print("Parsed values: knob=%f, shoot=%d, move=%d, change=%d, switch=%d, flashlight=%f, reload=%d" % [knobValue, shootState, moveState, changeDirectionState, switchState, flashlightValue, reloadValue])
	
	# Process the received commands.
	if shootState == 1:
		shoot()
	if changeDirectionState == 1:
		changeAxis()
	if reloadValue == 1:
		reloaded()
	if moveState:
		move(switchState)
	
	# Adjust additional parameters.
	setFlashlightValue(flashlightValue)
	resourceDistribution(knobValue)
	write_message()

# Below are the stub functions calling the parent node's game logic.
func shoot() -> void:
	parentPlayerNode.Shoot()

func changeAxis() -> void:
	parentPlayerNode.changeRotation()

func reloaded() -> void:
	parentPlayerNode.Reload()

func move(value: int) -> void:
	parentPlayerNode.move(value)

func resourceDistribution(dist: float) -> void:
	parentPlayerNode.SetDistribution(dist / 1000)

func setFlashlightValue(value: float) -> void:
	var f : SpotLight3D = parentPlayerNode._flashlight;
	if value < 1020:
		f.light_energy = 0;
	else:
		f.light_energy = 64;


func send_reload_prompt() -> void:
	# print("reload prompt sent")
	# write_serial("r")
	currentString += "r"

func send_light_indicator(direction: int) -> void:
	# print("indicator: " + str(direction))
	# write_serial("a" + str(direction) + "a")
	currentString += "a" + str(direction) + "a"

func send_buzzer_buzz(turnOffAfterTime : float) -> void:
	# print("buzzer")
	# write_serial("b")
	currentString += "b"

func write_message() -> void:
	# currentString += "\n"
	# print("Trying...")
	if currentString.is_empty():
		return
	socket.send_text(currentString)
	print(currentString)
	currentString = ""
