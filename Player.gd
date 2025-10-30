extends Area2D

@export var MaxSpeed: float = 450.0
@export var Acceleration: float = 2000.0
@export var Drag: float = 1200.0
@export var AnimationSpeed: float = 8.0

@export var BulletScene: PackedScene
@export var FireRate: float = 0.3

var _screen_size: Vector2
var _anim: AnimatedSprite2D
var _sprite_fallback: Sprite2D

var _idle_anim := ""
var _run_anim := ""

var _manual_anim := ""
var _manual_frame := 0
var _manual_frame_count := 0
var _manual_timer := 0.0

var _velocity: Vector2 = Vector2.ZERO
var _fire_timer := 0.0

var _is_invincible := false
var _invincibility_time := 1.0
var _invincibility_timer := 0.0
var _flash_effect: ColorRect

func _ready() -> void:
	_screen_size = get_viewport().get_visible_rect().size
	add_to_group("player")
	area_entered.connect(_on_area_entered)

	_anim = get_node_or_null("AnimatedSprite2D")
	_sprite_fallback = get_node_or_null("Sprite2D")

	if _anim and _anim.sprite_frames:
		var names: Array = _anim.sprite_frames.get_animation_names()
		_run_anim = _find_preferred(names, ["run", "walk", "move"]) 
		_idle_anim = _find_preferred(names, ["idle", "stand", "default", "rest"]) 
		if _run_anim.is_empty() and names.size() > 0:
			_run_anim = names[0]
		if _idle_anim.is_empty() and names.size() > 0:
			_idle_anim = names[0]
		_anim.speed_scale = 0.0
		if not _idle_anim.is_empty():
			_set_manual_animation(_idle_anim)

func _process(delta: float) -> void:
	if _screen_size == Vector2.ZERO:
		_screen_size = get_viewport().get_visible_rect().size

	if _is_invincible:
		_invincibility_timer -= delta
		if _invincibility_timer <= 0.0:
			_is_invincible = false
			if _flash_effect:
				_flash_effect.queue_free()
				_flash_effect = null

	var input := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	if input.length() > 0.1:
		var dir := input.normalized()
		_velocity += dir * Acceleration * delta
		if _velocity.length() > MaxSpeed:
			_velocity = _velocity.normalized() * MaxSpeed
	else:
		var drop := Drag * delta
		var speed := _velocity.length()
		if speed <= drop:
			_velocity = Vector2.ZERO
		else:
			_velocity -= _velocity.normalized() * drop

	position += _velocity * delta

	var target_anim := ""
	if _velocity.length() > 10.0:
		target_anim = _run_anim if not _run_anim.is_empty() else _idle_anim
	else:
		target_anim = _idle_anim

	if _anim and not target_anim.is_empty() and _manual_anim != target_anim:
		_set_manual_animation(target_anim)

	if _anim and _manual_frame_count > 0:
		_manual_timer += delta
		var interval := 1.0 / AnimationSpeed if AnimationSpeed > 0.0 else 0.125
		if _manual_timer >= interval:
			_manual_timer -= interval
			_manual_frame = (_manual_frame + 1) % _manual_frame_count
			_anim.frame = _manual_frame

	position = position.clamp(Vector2.ZERO, _screen_size)

	if BulletScene and FireRate > 0.0:
		if Input.is_action_pressed("ui_accept"):
			_fire_timer -= delta
			if _fire_timer <= 0.0:
				_fire_timer = FireRate
				var inst := BulletScene.instantiate()
				if inst is Node2D:
					inst.position = global_position + Vector2(20, 0)
					var root := get_tree().current_scene
					if root:
						root.add_child(inst)
					else:
						if get_parent():
							get_parent().add_child(inst)
		else:
			_fire_timer = 0.0

func _set_manual_animation(anim_name: String) -> void:
	if not _anim or not _anim.sprite_frames:
		return
	_manual_anim = anim_name
	_manual_frame = 0
	_manual_timer = 0.0
	if _anim.sprite_frames.has_animation(anim_name):
		_manual_frame_count = _anim.sprite_frames.get_frame_count(anim_name)
	else:
		_manual_frame_count = 0
	if _manual_frame_count > 0:
		_anim.frame = 0

func _find_preferred(names: Array, preferred: Array) -> String:
	if names.is_empty():
		return ""
	var lower := {}
	for n in names:
		lower[n.to_lower()] = n
	for p in preferred:
		if lower.has(p.to_lower()):
			return lower[p.to_lower()]
	return ""

func _on_area_entered(area: Area2D) -> void:
	if area.is_in_group("enemy_bullets") or area.is_in_group("enemies"):
		if not _is_invincible:
			_take_damage()

func _take_damage() -> void:
	_is_invincible = true
	_invincibility_timer = _invincibility_time
	_flash_effect = ColorRect.new()
	_flash_effect.color = Color(1, 0, 0, 0.3)
	_flash_effect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_flash_effect)
	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("OnPlayerHit"):
		game_manager.OnPlayerHit()
