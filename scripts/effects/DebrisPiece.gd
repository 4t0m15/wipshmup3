extends Node2D
class_name DebrisPiece

@export var Lifetime: float = 1.2
@export var InitialSpeed: float = 300.0
@export var AngularSpeed: float = 360.0
@export var Gravity: float = 400.0
@export var FadeOutTime: float = 0.25

var _vel: Vector2 = Vector2.ZERO
var _angular_vel: float = 0.0
var _lifetime: float = 0.0
var _sprite: CanvasItem

func _ready() -> void:
	_lifetime = Lifetime
	_sprite = get_node_or_null("Sprite2D") as CanvasItem
	if _sprite == null:
		# Create a small polygon square if no sprite provided
		var poly := Polygon2D.new()
		poly.polygon = PackedVector2Array([Vector2(-3, -3), Vector2(3, -3), Vector2(3, 3), Vector2(-3, 3)])
		add_child(poly)
		_sprite = poly

func setup(direction: Vector2, color: Color=Color(1,1,1,1)) -> void:
	_vel = direction.normalized() * InitialSpeed * randf_range(0.6, 1.2)
	_angular_vel = deg_to_rad(AngularSpeed) * randf_range(0.5, 1.5) * ((-1.0) if randf() < 0.5 else 1.0)
	if _sprite:
		_sprite.modulate = color

func _process(delta: float) -> void:
	# simple ballistic motion
	_vel.y += Gravity * delta
	rotation += _angular_vel * delta
	position += _vel * delta
	_lifetime -= delta
	if _lifetime <= FadeOutTime and _sprite:
		var t: float = clamp(1.0 - ((_lifetime) / max(FadeOutTime, 0.001)), 0.0, 1.0)
		_sprite.modulate.a = 1.0 - t
	if _lifetime <= 0.0:
		queue_free()
