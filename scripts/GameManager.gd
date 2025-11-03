extends Node

@export var EnemyScene: PackedScene = preload("res://scenes/enemy.tscn")
@export var EnemyBulletScene: PackedScene = preload("res://scenes/enemy_bullet.tscn")
@export var BossScene: PackedScene = preload("res://scenes/boss.tscn")
@export var PlayerHealth: int = 5
@export var SpawnRate: float = 3.5
@export var BossSpawnTime: float = 30.0

var _screen_size: Vector2
var _spawn_timer := 0.0
var _boss_spawn_timer := 0.0
var _boss_spawned := false
var _current_health := 0
var _game_over := false
var _current_score := 0
var _high_score := 0
var _hud: Node

func _ready() -> void:
	_screen_size = get_viewport().get_visible_rect().size
	if _screen_size == Vector2.ZERO:
		_screen_size = Vector2(1024, 600)

	_current_health = PlayerHealth
	add_to_group("game_manager")

	var current := get_tree().current_scene
	if current:
		_hud = current.get_node_or_null("HUD")
		if not _hud:
			_hud = current.get_node_or_null("./HUD")
	_spawn_timer = SpawnRate
	_boss_spawn_timer = BossSpawnTime

	_update_health()
	_update_score()
	_hide_game_over()

func _process(delta: float) -> void:
	if _game_over:
		return

	# Spawn enemies
	_spawn_timer -= delta
	if _spawn_timer <= 0.0:
		_spawn_timer = SpawnRate
		_spawn_enemy()

	# Spawn boss once
	if not _boss_spawned:
		_boss_spawn_timer -= delta
		if _boss_spawn_timer <= 0.0:
			_spawn_boss()
			_boss_spawned = true

func _spawn_enemy() -> void:
	if not EnemyScene:
		return

	var enemy := EnemyScene.instantiate()
	if enemy is Node2D:
		var random_y := randf_range(50.0, _screen_size.y - 50.0)
		enemy.position = Vector2(_screen_size.x + 50.0, random_y)
		get_tree().current_scene.add_child(enemy)

func _spawn_boss() -> void:
	if not BossScene:
		return

	var boss := BossScene.instantiate()
	if boss is Node2D:
		boss.position = Vector2(_screen_size.x + 200.0, _screen_size.y / 2.0)
		get_tree().current_scene.add_child(boss)

func OnPlayerHit() -> void:
	if _game_over:
		return

	_current_health -= 1
	_update_health()

	if _current_health <= 0:
		_game_over_fn()

func OnEnemyDestroyed(score_value: int) -> void:
	_current_score += score_value
	_update_score()

func GetEnemyBulletScene() -> PackedScene:
	return EnemyBulletScene

func _game_over_fn() -> void:
	_game_over = true
	if _current_score > _high_score:
		_high_score = _current_score

	_show_game_over()

	var timer := Timer.new()
	timer.wait_time = 2.0
	timer.one_shot = true
	timer.timeout.connect(_restart_game)
	add_child(timer)
	timer.start()

func _restart_game() -> void:
	_current_health = PlayerHealth
	_current_score = 0
	_game_over = false
	_boss_spawned = false
	_boss_spawn_timer = BossSpawnTime

	_update_health()
	_update_score()
	_hide_game_over()

	# Reacquire HUD in case scene reloaded
	var current := get_tree().current_scene
	if current:
		_hud = current.get_node_or_null("HUD")

	var player := get_tree().get_first_node_in_group("player")
	if player is Node2D:
		player.position = Vector2(100.0, _screen_size.y / 2.0)

	_clear_all_entities()

func _clear_all_entities() -> void:
	for group in ["enemies", "player_bullets", "enemy_bullets"]:
		for entity in get_tree().get_nodes_in_group(group):
			if entity is Node:
				entity.queue_free()

func _update_score() -> void:
	if _hud and _hud.has_method("UpdateScore"):
		_hud.UpdateScore(_current_score, _high_score)

func _update_health() -> void:
	if _hud and _hud.has_method("UpdateHealth"):
		_hud.UpdateHealth(_current_health)

func _show_game_over() -> void:
	if _hud and _hud.has_method("ShowGameOver"):
		_hud.ShowGameOver(_current_score, _high_score)

func _hide_game_over() -> void:
	if _hud and _hud.has_method("HideGameOver"):
		_hud.HideGameOver()
