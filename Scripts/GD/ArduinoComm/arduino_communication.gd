extends SerComm

# We don't need Process function. The documentation says the message receiving is done in messsage_read
func _ready() -> void:
	if open_serial():
		on_message.connect(message_read)
	
# func _process(delta: float) -> void:
	# pass
	
func _exit_tree() -> void:
	close_serial()



func message_read(m):
	# The documentation says the message receiving is done in messsage_read
	# DON'T do process()
	print(m)


# For calling GD stuff from C# we do:
# 		OBJECT.Call("method_name", param1, param2, ...);
# 		OBJECT.Call("method_name2");

# For calling C# stuff from GD we do:
#		OBJECT.method_name(params)
