extends Damageable

@export var EntrySpeed: float = 200.0
@export var HorizontalTargetOffset: float = 300.0
@export var HoverAmplitude: int = 60
@export var HoverFrequency: int = 1
@export var FireInterval: float = 0.08
@export var AngularVelocity: float = 90.0
@export var BulletsPerShot: int = 6
@export var BulletSpeed: int = 300
@export var BulletScene: PackedScene

var _screen_size: Vector2
var _state: String = "entering"
var _target_x: float = 0.0
var _hover_time := 0.0
var _fire_timer := 0.0
var _angle_deg := 0.0
var _bullet_scene: PackedScene

func _ready() -> void:
	super._ready()
	_screen_size = get_viewport().get_visible_rect().size
	if _screen_size == Vector2.ZERO:
		_screen_size = Vector2(1024, 600)

	add_to_group("boss")

	# Load bullet scene
	if BulletScene:
		_bullet_scene = BulletScene
	else:
		var gm := get_node_or_null("/root/GameManager")
		if gm and gm.has_method("GetEnemyBulletScene"):
			_bullet_scene = gm.GetEnemyBulletScene()

	_target_x = _screen_size.x - HorizontalTargetOffset
	_angle_deg = randi() % 360
	_fire_timer = FireInterval

	area_entered.connect(_on_area_entered)

func _process(delta: float) -> void:
	super._process(delta)

	if _state == "entering":
		position.x -= EntrySpeed * delta
		if position.x <= _target_x:
			position.x = _target_x
			_state = "active"
	elif _state == "active":
		# Hover vertically
		_hover_time += delta
		var hover_offset := sin(_hover_time * TAU * HoverFrequency) * HoverAmplitude
		position.y = clamp(_screen_size.y / 2.0 + hover_offset, 0.0, _screen_size.y)

		# Fire spiral bullets
		_fire_timer -= delta
		if _fire_timer <= 0.0:
			_fire_timer += FireInterval
			_shoot_spiral()

		_angle_deg = fmod(_angle_deg + AngularVelocity * delta, 360.0)

func _shoot_spiral() -> void:
	if not _bullet_scene:
		return

	var count: int = max(1, BulletsPerShot)
	var angle_step: float = 360.0 / float(count)
	for i in range(BulletsPerShot):
		var angle_deg: float = _angle_deg + float(i) * angle_step
		var bullet := _bullet_scene.instantiate()
		if bullet is Node2D:
			bullet.position = global_position
			if bullet.has_method("set"):
				bullet.set("Angle", angle_deg)
				bullet.set("Speed", BulletSpeed)
			get_tree().current_scene.add_child(bullet)

func _on_area_entered(area: Area2D) -> void:
	if area.is_in_group("player_bullets"):
		var dmg := 1
		if area.has_method("get"):
			dmg = area.get("Damage") if area.get("Damage") else 1
		TakeDamage(dmg)
		area.queue_free()
