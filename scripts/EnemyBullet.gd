extends Area2D

@export var Speed: float = 300.0
@export var Damage: int = 1
# Angle in degrees. Default 180 points to the left (same behavior as before).
@export var Angle: float = 180.0

var _screen_size: Vector2
var _velocity: Vector2 = Vector2.ZERO

func _ready() -> void:
    _screen_size = get_viewport().get_visible_rect().size
    add_to_group("enemy_bullets")
    area_entered.connect(_on_area_entered)
    # Compute initial velocity from exported angle so bullets can be spawned in arbitrary directions (useful for boss spiral patterns).
    var rad := deg_to_rad(Angle)
    _velocity = Vector2(cos(rad), sin(rad)) * Speed

func _process(delta: float) -> void:
    position += _velocity * delta
    if position.x < -50.0 or position.x > _screen_size.x + 50.0 or position.y < -50.0 or position.y > _screen_size.y + 50.0:
        queue_free()

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("player"):
        queue_free()
