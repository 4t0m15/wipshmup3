extends Area2D

@export var Speed: float = 300.0
@export var Damage: int = 1

var _screen_size: Vector2

func _ready() -> void:
    _screen_size = get_viewport().get_visible_rect().size
    add_to_group("enemy_bullets")
    area_entered.connect(_on_area_entered)

func _process(delta: float) -> void:
    position += Vector2.LEFT * Speed * delta
    if position.x < -50.0:
        queue_free()

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("player"):
        queue_free()

