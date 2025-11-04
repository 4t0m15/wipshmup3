extends Area2D
class_name Damageable

@export var MaxHealth: int = 3
@export var ScoreValue: int = 100

var _health: int
var _visual: CanvasItem
var _is_flashing := false
var _flash_timer := 0.0

func _ready() -> void:
	_health = MaxHealth
	add_to_group("enemies")

	# Find visual node
	_visual = get_node_or_null("Sprite2D")
	if not _visual:
		_visual = get_node_or_null("ColorRect")
	if not _visual:
		_visual = get_node_or_null("TextureRect")

func _process(delta: float) -> void:
	if _is_flashing:
		_flash_timer -= delta
		if _flash_timer <= 0.0:
			_is_flashing = false
			if _visual:
				_visual.modulate = Color(1, 1, 1, 1)

func TakeDamage(damage: int) -> void:
	_health -= damage
	_flash_damage()

	if _health <= 0:
		_on_death()

func _flash_damage() -> void:
	_is_flashing = true
	_flash_timer = 0.1
	if _visual:
		_visual.modulate = Color(1, 0, 0, 1)

func _on_death() -> void:
	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("OnEnemyDestroyed"):
		game_manager.OnEnemyDestroyed(ScoreValue)

	# Spawn debris pieces that fly apart
	_spawn_debris()
	queue_free()

func GetHealth() -> int:
	return _health

func _spawn_debris() -> void:
	var count := 8
	var base_color := Color(1, 1, 1, 1)
	if _visual:
		base_color = _visual.modulate
	for i in range(count):
		var piece := DebrisPiece.new()
		# Try to copy a tiny visual chunk by adding a Sprite2D if possible
		var spr := Sprite2D.new()
		if _visual is Sprite2D:
			spr.texture = (_visual as Sprite2D).texture
			spr.region_enabled = true
			var rx := randi() % 4
			var ry := randi() % 4
			spr.region_rect = Rect2(Vector2(rx * 6, ry * 6), Vector2(6, 6))
			spr.scale = Vector2(0.3, 0.3)
			piece.add_child(spr)
		piece.position = global_position
		get_tree().current_scene.add_child(piece)
		var angle := deg_to_rad(float(i) * (360.0 / float(count))) + randf_range(-0.3, 0.3)
		piece.setup(Vector2(cos(angle), sin(angle)), base_color)
