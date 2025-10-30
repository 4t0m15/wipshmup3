extends Node

@export var EnemyScene: PackedScene
@export var EnemyBulletScene: PackedScene
@export var PlayerHealth: int = 5
@export var SpawnRate: float = 3.5

var _screen_size: Vector2
var _spawn_timer := 0.0
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

    if not EnemyScene:
        EnemyScene = load("res://enemy.tscn")
    if not EnemyBulletScene:
        EnemyBulletScene = load("res://enemy_bullet.tscn")

    _hud = get_tree().current_scene.get_node_or_null("HUD")
    _spawn_timer = SpawnRate
    _update_health()
    _update_score()
    _hide_game_over()

func _process(delta: float) -> void:
    if _game_over:
        return
    if _screen_size == Vector2.ZERO:
        _screen_size = get_viewport().get_visible_rect().size
        if _screen_size == Vector2.ZERO:
            _screen_size = Vector2(1024, 600)
    _spawn_timer -= delta
    if _spawn_timer <= 0.0:
        _spawn_timer = SpawnRate
        _spawn_enemy()

func _spawn_enemy() -> void:
    if not EnemyScene:
        return
    var enemy := EnemyScene.instantiate()
    if enemy is Node2D:
        var random_y := randf_range(50.0, _screen_size.y - 50.0)
        enemy.position = Vector2(_screen_size.x + 50.0, random_y)
        get_tree().current_scene.add_child(enemy)

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
    _update_health()
    _update_score()
    _hide_game_over()
    var player := get_tree().get_first_node_in_group("player")
    if player is Node2D:
        player.position = Vector2(100.0, _screen_size.y / 2.0)
    _clear_all_entities()

func _clear_all_entities() -> void:
    for e in get_tree().get_nodes_in_group("enemies"):
        if e is Node:
            e.queue_free()
    for b in get_tree().get_nodes_in_group("player_bullets"):
        if b is Node:
            b.queue_free()
    for b in get_tree().get_nodes_in_group("enemy_bullets"):
        if b is Node:
            b.queue_free()

func GetEnemyBulletScene() -> PackedScene:
    return EnemyBulletScene

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

