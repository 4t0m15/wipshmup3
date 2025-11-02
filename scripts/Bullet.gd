extends Area2D

@export var Speed: float = 300.0
@export var Damage: int = 1
@export var Angle: float = 0.0  # 0 = right, 90 = down, 180 = left, 270 = up
@export var BulletType: String = "player"  # "player" or "enemy"

var _screen_size: Vector2
var _velocity: Vector2 = Vector2.ZERO

func _ready() -> void:
	_screen_size = get_viewport().get_visible_rect().size

	# Add to appropriate group based on type
	if BulletType == "player":
		add_to_group("player_bullets")
	else:
		add_to_group("enemy_bullets")

	# Calculate velocity from angle
	var rad := deg_to_rad(Angle)
	_velocity = Vector2(cos(rad), sin(rad)) * Speed

	area_entered.connect(_on_area_entered)

func _process(delta: float) -> void:
	position += _velocity * delta

	# Remove bullet when off-screen
	if position.x < -50.0 or position.x > _screen_size.x + 50.0 or \
	   position.y < -50.0 or position.y > _screen_size.y + 50.0:
		queue_free()

func _on_area_entered(area: Area2D) -> void:
	if BulletType == "player" and area.is_in_group("enemies"):
		if area.has_method("TakeDamage"):
			area.TakeDamage(Damage)
		queue_free()
	elif BulletType == "enemy" and area.is_in_group("player"):
		queue_free()
