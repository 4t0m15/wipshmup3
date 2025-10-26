using Godot;

public partial class Mob : RigidBody2D
{
    [Export] public float MinSpeed { get; set; } = 150f;
    [Export] public float MaxSpeed { get; set; } = 250f;
    
    public override void _EnterTree()
    {
        // Safety: ignore parent transforms if accidentally parented under Player
        TopLevel = true;
    }

    public override void _Ready()
    {
        // Handle either "AnimatedSprite2D" or "AnimatedSprite2D2"
        var anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")
            ?? GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D2");

        if (anim != null && anim.SpriteFrames != null)
        {
            string[] mobTypes = anim.SpriteFrames.GetAnimationNames();
            if (mobTypes.Length > 0)
                anim.Play(mobTypes[GD.Randi() % mobTypes.Length]);
        }
        else
        {
            GD.PushWarning("[Mob] AnimatedSprite2D not found or SpriteFrames missing.");
        }
    }

    public void Initialize(Vector2 direction)
    {
        if (direction == Vector2.Zero)
            direction = Vector2.Right;

        direction = direction.Normalized();
        float speed = Mathf.Lerp(MinSpeed, MaxSpeed, (float)GD.Randf());
        LinearVelocity = direction * speed;
        Rotation = direction.Angle();
    }

    private void OnVisibleOnScreenNotifier2DScreenExited()
    {
        QueueFree();
    }
}