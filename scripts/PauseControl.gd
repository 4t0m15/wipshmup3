extends Node

var _pause_label: Label

func _ready() -> void:
	_pause_label = get_node_or_null("PauseLabel")
	if _pause_label:
		_pause_label.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		get_tree().paused = not get_tree().paused
		if _pause_label:
			_pause_label.visible = get_tree().paused
