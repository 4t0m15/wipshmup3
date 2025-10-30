extends Area2D

@export var Speed: float = 200.0
@export var Health: int = 3
@export var ScoreValue: int = 100
@export var FireRate: float = 1.5

var _screen_size: Vector2
var _fire_timer := 0.0
var _bullet_scene: PackedScene
var _visual: CanvasItem
var _is_flashing := false
var _flash_timer := 0.0

func _ready() -> void:
    _screen_size = get_viewport().get_visible_rect().size
    add_to_group("enemies")

    var game_manager := get_node_or_null("/root/GameManager")
    if game_manager and game_manager.has_method("GetEnemyBulletScene"):
        _bullet_scene = game_manager.GetEnemyBulletScene()

    _visual = get_node_or_null("Sprite2D")
    if not _visual:
        _visual = get_node_or_null("ColorRect")

    _fire_timer = FireRate

func _process(delta: float) -> void:
    position += Vector2.LEFT * Speed * delta

    if _is_flashing:
        _flash_timer -= delta
        if _flash_timer <= 0.0:
            _is_flashing = false
            if _visual:
                _visual.modulate = Color(1, 1, 1, 1)

    if _bullet_scene:
        _fire_timer -= delta
        if _fire_timer <= 0.0:
            _fire_timer = FireRate
            _shoot()

    if position.x < -100.0:
        queue_free()

func TakeDamage(damage: int) -> void:
    Health -= damage
    _is_flashing = true
    _flash_timer = 0.1
    if _visual:
        _visual.modulate = Color(1, 0, 0, 1)
    if Health <= 0:
        _destroy()

func _destroy() -> void:
    var game_manager := get_node_or_null("/root/GameManager")
    if game_manager and game_manager.has_method("OnEnemyDestroyed"):
        game_manager.OnEnemyDestroyed(ScoreValue)
    queue_free()

func _shoot() -> void:
    if not _bullet_scene:
        return
    var bullet := _bullet_scene.instantiate()
    if bullet is Node2D:
        bullet.position = global_position + Vector2(-40, 0)
        get_tree().current_scene.add_child(bullet)

