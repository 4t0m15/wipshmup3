extends Node2D

var pause_label: Label

func _ready() -> void:
    pause_label = get_node_or_null("PauseLabel")
    if pause_label:
        pause_label.visible = false
    Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)

func _unhandled_input(event):
    if event.is_action_pressed("ui_cancel"):
        get_tree().paused = not get_tree().paused
        if pause_label:
            pause_label.visible = get_tree().paused
        # Optional: make sure UI is unpaused
        get_tree().get_root().set_process_input(true)

func _process(_delta):
    # Just in case label gets out of sync
    if pause_label:
        pause_label.visible = get_tree().paused
