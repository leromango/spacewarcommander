extends Node

func playSound(sound : AudioStream) -> void:
	var createdAudioObject : AudioStreamPlayer = AudioStreamPlayer.new()
	createdAudioObject.stream = sound
	createdAudioObject.finished.connect(Callable(createdAudioObject, "queue_free"))
	get_tree().get_current_scene().add_child(createdAudioObject)
	createdAudioObject.play()


func playSound3D(sound : AudioStream, location : Vector3, attachToNode : Node3D = null, volumeDB : float = 0) -> void:
	var createdAudioObject : AudioStreamPlayer3D = AudioStreamPlayer3D.new()
	createdAudioObject.volume_db = volumeDB
	createdAudioObject.stream = sound
	createdAudioObject.global_position = location
	if (attachToNode != null):
		attachToNode.add_child(attachToNode)
	else:
		get_tree().get_current_scene().add_child(createdAudioObject)
	createdAudioObject.finished.connect(Callable(createdAudioObject, "queue_free"))
	createdAudioObject.play()
