extends Area2D

@export var Speed: float = 300.0
@export var Damage: int = 1
# Angle in degrees. Default 180 points to the left (same behavior as before).
@export var Angle: float = 180.0
# Homing behavior
@export var Homing: bool = false
@export var TurnSpeed: float = 240.0  # degrees per second
@export var TargetPath: NodePath
@export var HomingDuration: float = 2.0  # seconds; 0 disables homing immediately

var _screen_size: Vector2
var _velocity: Vector2 = Vector2.ZERO
var _target: Node2D
var _homing_timer: float = 0.0

func _ready() -> void:
    _screen_size = get_viewport().get_visible_rect().size
    add_to_group("enemy_bullets")
    area_entered.connect(_on_area_entered)
    # Compute initial velocity from exported angle so bullets can be spawned in arbitrary directions (useful for boss spiral patterns).
    var rad := deg_to_rad(Angle)
    _velocity = Vector2(cos(rad), sin(rad)) * Speed
    # Acquire target if homing
    if Homing:
        _homing_timer = max(0.0, HomingDuration)
        if TargetPath != NodePath(""):
            var node := get_node_or_null(TargetPath)
            if node is Node2D:
                _target = node
        if not _target:
            _target = get_tree().get_first_node_in_group("player") as Node2D

func _process(delta: float) -> void:
    # Adjust heading toward target if homing; disable after duration
    if Homing:
        if _homing_timer > 0.0:
            _homing_timer -= delta
            if _homing_timer <= 0.0:
                Homing = false
        if Homing and _target:
            var desired := (_target.global_position - global_position)
            if desired.length() > 0.001:
                var desired_dir := desired.normalized()
                var cur_speed := _velocity.length()
                if cur_speed <= 0.001:
                    cur_speed = Speed
                var current_dir := _velocity.normalized()
                var max_turn := deg_to_rad(TurnSpeed) * delta
                var angle: float = current_dir.angle_to(desired_dir)
                var clamped: float = clamp(angle, -max_turn, max_turn)
                var new_dir := current_dir.rotated(clamped)
                _velocity = new_dir * cur_speed

    position += _velocity * delta
    # Do not despawn bullets off-screen anymore this was buggy.

func _on_area_entered(area: Area2D) -> void:
    if area.is_in_group("player"):
        # Do not despawn on hitting player; damage is handled by the player.
        pass
