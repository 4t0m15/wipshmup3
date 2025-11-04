extends Damageable

@export var Speed: float = 250.0
@export var FireRate: float = 1.2

var _screen_size: Vector2
var _fire_timer := 0.0
var _bullet_scene: PackedScene

func _ready() -> void:
	super._ready()
	_screen_size = get_viewport().get_visible_rect().size

	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("GetEnemyBulletScene"):
		_bullet_scene = game_manager.GetEnemyBulletScene()

	_fire_timer = FireRate

func _process(delta: float) -> void:
	super._process(delta)

	# Move left
	position += Vector2.LEFT * Speed * delta

	# Shoot periodically
	if _bullet_scene:
		_fire_timer -= delta
		if _fire_timer <= 0.0:
			_fire_timer = FireRate
			_shoot()

	# Remove when off-screen
	if position.x < -100.0:
		queue_free()

func _shoot() -> void:
	if not _bullet_scene:
		return

	var bullet := _bullet_scene.instantiate()
	if bullet is Node2D:
		bullet.position = global_position + Vector2(-40, 0)
		get_tree().current_scene.add_child(bullet)
