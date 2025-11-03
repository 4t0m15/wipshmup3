extends Node

var _pause_label: Label
@onready var _tree := get_tree()

func _ready() -> void:
	_pause_label = get_node_or_null("PauseLabel")
	if _pause_label:
		_pause_label.visible = false
	set_process_unhandled_input(true)
	process_mode = Node.PROCESS_MODE_ALWAYS

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_cancel"):
		_tree.paused = not _tree.paused
		if _pause_label:
			_pause_label.visible = _tree.paused
