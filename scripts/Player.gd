extends Area2D

@export var MaxSpeed: float = 450.0
@export var Acceleration: float = 2000.0
@export var Drag: float = 1200.0
@export var BulletScene: PackedScene
@export var FireRate: float = 0.15

var _screen_size: Vector2
var _velocity: Vector2 = Vector2.ZERO
var _fire_timer := 0.0
var _is_invincible := false
var _invincibility_timer := 0.0
var _flash_effect: ColorRect

func _ready() -> void:
	_screen_size = get_viewport().get_visible_rect().size
	add_to_group("player")
	area_entered.connect(_on_area_entered)

func _process(delta: float) -> void:
	# Update screen size if needed
	if _screen_size == Vector2.ZERO:
		_screen_size = get_viewport().get_visible_rect().size

	# Handle invincibility
	if _is_invincible:
		_invincibility_timer -= delta
		if _invincibility_timer <= 0.0:
			_is_invincible = false
			if _flash_effect:
				_flash_effect.queue_free()
				_flash_effect = null

	# Movement
	var input := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	if input.length() > 0.0:
		_velocity += input.normalized() * Acceleration * delta
		if _velocity.length() > MaxSpeed:
			_velocity = _velocity.normalized() * MaxSpeed
	else:
		var speed := _velocity.length()
		if speed > 0.0:
			var drop: float = minf(Drag * delta, speed)
			_velocity -= _velocity.normalized() * drop

	position += _velocity * delta
	position = position.clamp(Vector2.ZERO, _screen_size)

	# Shooting
	if BulletScene and Input.is_action_pressed("ui_accept"):
		_fire_timer -= delta
		if _fire_timer <= 0.0:
			_fire_timer = FireRate
			_shoot()
	else:
		_fire_timer = 0.0

func _shoot() -> void:
	var bullet := BulletScene.instantiate()
	if bullet is Node2D:
		bullet.position = global_position + Vector2(20, 0)
		get_tree().current_scene.add_child(bullet)

func _on_area_entered(area: Area2D) -> void:
	if (area.is_in_group("enemy_bullets") or area.is_in_group("enemies")) and not _is_invincible:
		_take_damage()

func _take_damage() -> void:
	_is_invincible = true
	_invincibility_timer = 1.0

	# Create flash effect
	_flash_effect = ColorRect.new()
	_flash_effect.color = Color(1, 0, 0, 0.3)
	_flash_effect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_flash_effect)

	# Notify game manager
	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("OnPlayerHit"):
		game_manager.OnPlayerHit()
