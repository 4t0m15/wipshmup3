using Godot;

public partial class Mob : RigidBody2D
{
    [Export] public float MinSpeed { get; set; } = 150f;
    [Export] public float MaxSpeed { get; set; } = 250f;

    public override void _Ready()
    {
        var anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (anim?.SpriteFrames != null)
        {
            var mobTypes = anim.SpriteFrames.GetAnimationNames();
            if (mobTypes.Length > 0)
                anim.Play(mobTypes[GD.Randi() % mobTypes.Length]);
        }
    }

    public void Initialize(Vector2 direction)
    {
        if (direction == Vector2.Zero) direction = Vector2.Right;
        direction = direction.Normalized();
        float speed = Mathf.Lerp(MinSpeed, MaxSpeed, (float)GD.Randf());
        LinearVelocity = direction * speed;
        Rotation = direction.Angle();
    }

    private void OnVisibleOnScreenNotifier2DScreenExited() => QueueFree();
}