extends Area2D

# Large boss that moves into the screen, holds position, and fires a spiral pattern of enemy bullets.

@export var MaxHealth: int = 80
@export var EntrySpeed: float = 200.0        # Speed while entering from the right
@export var HorizontalTargetOffset: float = 300.0  # How far from the right edge the boss will stop
@export var HoverAmplitude: float = 60.0     # Vertical hover amplitude (pixels)
@export var HoverFrequency: float = 1.0      # Hover speed (cycles per second)
@export var FireInterval: float = 0.08       # Time (s) between spiral shots (small -> dense spiral)
@export var AngularVelocity: float = 90.0    # Degrees per second the spiral angle advances
@export var BulletsPerShot: int = 6          # How many bullets per emission (ring slice)
@export var BulletSpeed: float = 300.0       # Speed for bullets spawned by the boss
@export var ScoreValue: int = 1000
@export var BossName: String = "BOSS"

# Optional external bullet scene; if not set, we will attempt to use GameManager's EnemyBulletScene
@export var BulletScene: PackedScene

# Internal state
var _screen_size: Vector2
var _health: int
var _state: String = "entering"  # entering, active, dead
var _target_x: float = 0.0
var _enter_direction := Vector2.LEFT
var _hover_time := 0.0
var _fire_timer := 0.0
var _angle_deg := 0.0
var _bullet_scene: PackedScene
var _visual: CanvasItem
var _is_flashing := false
var _flash_timer := 0.0

func _ready() -> void:
    _screen_size = get_viewport().get_visible_rect().size
    if _screen_size == Vector2.ZERO:
        _screen_size = Vector2(1024, 600)

    _health = MaxHealth
    add_to_group("enemies")
    add_to_group("boss")

    # Load bullet scene from export or from GameManager
    if BulletScene:
        _bullet_scene = BulletScene
    else:
        var gm := get_node_or_null("/root/GameManager")
        if gm and gm.has_method("GetEnemyBulletScene"):
            _bullet_scene = gm.GetEnemyBulletScene()

    # Compute where to stop horizontally (relative to right edge)
    _target_x = _screen_size.x - HorizontalTargetOffset

    # Visual node if present (Sprite2D, TextureRect, etc.)
    _visual = get_node_or_null("Sprite2D")
    if not _visual:
        _visual = get_node_or_null("TextureRect")
    if not _visual:
        _visual = get_node_or_null("ColorRect")

    # Collision detection: listen for area_entered to handle player bullets
    area_entered.connect(_on_area_entered)

    # Start with an angle offset so the spiral doesn't always begin the same
    _angle_deg = randi() % 360
    _fire_timer = FireInterval

func _process(delta: float) -> void:
    if _state == "dead":
        return

    # Flashing visual on damage
    if _is_flashing:
        _flash_timer -= delta
        if _flash_timer <= 0.0:
            _is_flashing = false
            if _visual:
                _visual.modulate = Color(1, 1, 1, 1)

    if _state == "entering":
        _process_entering(delta)
    elif _state == "active":
        _process_active(delta)

func _process_entering(delta: float) -> void:
    # Move left until we reach the target x coordinate
    position += _enter_direction * EntrySpeed * delta
    if position.x <= _target_x:
        position.x = _target_x
        _state = "active"
        # Reset timers for active behavior
        _hover_time = 0.0
        _fire_timer = FireInterval

func _process_active(delta: float) -> void:
    # Hover vertically using a simple sine wave
    _hover_time += delta
    var hover_offset := sin(_hover_time * TAU * HoverFrequency) * HoverAmplitude
    position.y = clamp(_screen_size.y / 2.0 + hover_offset, 0.0, _screen_size.y)

    # Spiral firing
    _fire_timer -= delta
    if _fire_timer <= 0.0:
        _fire_timer += FireInterval
        _shoot_spiral()

    # Advance spiral rotation
    _angle_deg = (_angle_deg + AngularVelocity * delta) % 360.0

func _shoot_spiral() -> void:
    if not _bullet_scene:
        return

    # Spawn BulletsPerShot bullets per shot, evenly spaced around the circle,
    # offset by the current spiral angle. Each bullet will be given an Angle property
    # in degrees (EnemyBullet.gd uses Angle to compute its velocity).
    var angle_step := 360.0 / max(1, BulletsPerShot)
    for i in BulletsPerShot > 0 ? range(0, BulletsPerShot) : []:
        var angle_deg := _angle_deg + i * angle_step
        var bullet := _bullet_scene.instantiate()
        if bullet is Node2D:
            bullet.position = global_position
            # Give the bullet the angle and speed. We set exported properties if available.
            if bullet.has_variable("Angle"):
                bullet.Angle = angle_deg
            else:
                # Try setting a velocity vector if the bullet supports it
                if bullet.has_variable("_velocity"):
                    var rad := deg2rad(angle_deg)
                    bullet._velocity = Vector2(cos(rad), sin(rad)) * BulletSpeed
            # If the bullet exposes Speed, set it so its ready() computes velocity correctly.
            if bullet.has_variable("Speed"):
                bullet.Speed = BulletSpeed
            get_tree().current_scene.add_child(bullet)

func TakeDamage(damage: int) -> void:
    if _state == "dead":
        return
    _health -= max(1, damage)
    _is_flashing = true
    _flash_timer = 0.12
    if _visual:
        _visual.modulate = Color(1, 0.5, 0.5, 1)

    if _health <= 0:
        _die()

func _die() -> void:
    _state = "dead"
    # Notify GameManager for scoring
    var gm := get_node_or_null("/root/GameManager")
    if gm and gm.has_method("OnEnemyDestroyed"):
        gm.OnEnemyDestroyed(ScoreValue)
    # Play death animation or effect if present (attempt to call a method on a child)
    if has_node("DeathEffect"):
        var effect = get_node("DeathEffect")
        if effect and effect.has_method("play"):
            effect.play()
    queue_free()

func _on_area_entered(area: Area2D) -> void:
    # Expect player bullets to be in the group "player_bullets"
    if area.is_in_group("player_bullets"):
        # Try to get damage value from the bullet, else assume 1
        var dmg := 1
        if area.has_variable("Damage"):
            dmg = int(area.Damage)
        elif area.has_method("GetDamage"):
            dmg = int(area.GetDamage())
        TakeDamage(dmg)
        # Remove the player bullet on hit
        if area is Node:
            area.queue_free()
