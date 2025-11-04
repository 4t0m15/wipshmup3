extends Area2D

@export var MaxSpeed: float = 450.0
@export var Acceleration: float = 2000.0
@export var Drag: float = 1200.0
@export var BulletScene: PackedScene
@export var FireRate: float = 0.08

var _screen_size: Vector2
var _velocity: Vector2 = Vector2.ZERO
var _fire_timer := 0.0
var _is_invincible := false
var _invincibility_timer := 0.0
var _sprite: CanvasItem
var _flash_timer_node: Timer
var _flash_ticks_remaining: int = 0
var _death_hidden := false

func _ready() -> void:
	_screen_size = get_viewport().get_visible_rect().size
	add_to_group("player")
	area_entered.connect(_on_area_entered)

	# Cache sprite for flashing
	_sprite = get_node_or_null("AnimatedSprite2D") as CanvasItem

func _process(delta: float) -> void:
	# Update screen size if needed
	if _screen_size == Vector2.ZERO:
		_screen_size = get_viewport().get_visible_rect().size

	# Handle invincibility
	if _is_invincible:
		_invincibility_timer -= delta
		if _invincibility_timer <= 0.0:
			_is_invincible = false
			# Ensure sprite returns to normal and flash timer is cleared
			if _sprite:
				_sprite.modulate = Color(1, 1, 1, 1)
			if _flash_timer_node:
				_flash_timer_node.stop()
				_flash_timer_node.queue_free()
				_flash_timer_node = null

	# Update firing cooldown timer
	if _fire_timer > 0.0:
		_fire_timer -= delta
		if _fire_timer < 0.0:
			_fire_timer = 0.0

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

	# Shooting (tap-only; holding does nothing)
	if BulletScene and Input.is_action_just_pressed("ui_accept"):
		if _fire_timer <= 0.0:
			_shoot()
			_fire_timer = FireRate

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

	# Flash the player sprite yellow twice
	_start_flash_twice()

	# Notify game manager
	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("OnPlayerHit"):
		game_manager.OnPlayerHit()

# Called by GameManager on game over to produce an extreme shatter effect
func SpawnDeathDebrisExtremeGrey() -> void:
	var node2d := self as Node2D
	if not node2d:
		return
	# Hide the sprite so debris reads clearly
	if _sprite:
		_sprite.visible = false
		_death_hidden = true

	var center := node2d.global_position
	var pieces := 36
	var tex: Texture2D
	var anim_node := get_node_or_null("AnimatedSprite2D") as AnimatedSprite2D
	if anim_node and anim_node.sprite_frames:
		var frames := anim_node.sprite_frames
		var anim := anim_node.animation
		var frame_idx := 0
		# Try to grab current frame if available
		if "frame" in anim_node:
			frame_idx = anim_node.frame
		tex = frames.get_frame_texture(anim, frame_idx)

	for i in range(pieces):
		var piece := DebrisPiece.new()
		# Make it more extreme
		piece.InitialSpeed = 750.0
		piece.AngularSpeed = 900.0
		piece.Gravity = 900.0
		piece.Lifetime = 1.6
		piece.FadeOutTime = 0.35
		if tex:
			var spr := Sprite2D.new()
			spr.texture = tex
			spr.region_enabled = true
			var tex_size := tex.get_size()
			var rw := 12.0
			var rh := 12.0
			var rx := randf_range(0.0, max(0.0, tex_size.x - rw))
			var ry := randf_range(0.0, max(0.0, tex_size.y - rh))
			spr.region_rect = Rect2(Vector2(rx, ry), Vector2(rw, rh))
			spr.scale = Vector2(0.35, 0.35)
			piece.add_child(spr)
		piece.position = center
		get_tree().current_scene.add_child(piece)
		var angle := randf() * TAU
		var dir := Vector2(cos(angle), sin(angle))
		piece.setup(dir, Color(0.6, 0.6, 0.6, 1.0))

func RestoreAfterDeathEffect() -> void:
	if _sprite and _death_hidden:
		_sprite.visible = true
		_sprite.modulate = Color(1, 1, 1, 1)
		_death_hidden = false

func _start_flash_twice() -> void:
	# Stop any existing flash timer
	if _flash_timer_node:
		_flash_timer_node.stop()
		_flash_timer_node.queue_free()
		_flash_timer_node = null

	_flash_ticks_remaining = 4  # Yellow, normal, yellow, normal
	_flash_timer_node = Timer.new()
	_flash_timer_node.wait_time = 0.08
	_flash_timer_node.one_shot = false
	add_child(_flash_timer_node)
	_flash_timer_node.timeout.connect(_on_flash_timer_timeout)
	_on_flash_timer_timeout()  # Apply first tick immediately
	_flash_timer_node.start()

func _on_flash_timer_timeout() -> void:
	if not _sprite:
		_sprite = get_node_or_null("AnimatedSprite2D") as CanvasItem
	if _sprite:
		# Toggle between yellow and white
		if _sprite.modulate == Color(1, 1, 1, 1):
			_sprite.modulate = Color(1, 1, 0, 1)
		else:
			_sprite.modulate = Color(1, 1, 1, 1)

	_flash_ticks_remaining -= 1
	if _flash_ticks_remaining <= 0 and _flash_timer_node:
		_flash_timer_node.stop()
		_flash_timer_node.queue_free()
		_flash_timer_node = null
		# Ensure end on normal color
		if _sprite:
			_sprite.modulate = Color(1, 1, 1, 1)
