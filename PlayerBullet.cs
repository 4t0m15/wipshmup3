using Godot;

public partial class PlayerBullet : Area2D
{
    [Export] public float Speed { get; set; } = 500f;
    [Export] public int Damage { get; set; } = 1;
    
    private Vector2 _direction = Vector2.Right;
    private Vector2 _screenSize;
    
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
        
        // Add to player bullet group
        AddToGroup("player_bullet");
        
        // Connect signals
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
        
        // Auto-remove after 3 seconds
        GetTree().CreateTimer(3.0).Timeout += () => QueueFree();
    }
    
    public override void _Process(double delta)
    {
        Position += _direction * Speed * (float)delta;
        
        // Remove if off screen
        if (Position.X < -50 || Position.X > _screenSize.X + 50)
        {
            QueueFree();
        }
    }
    
    public void SetDirection(Vector2 direction)
    {
        _direction = direction.Normalized();
        Rotation = _direction.Angle();
    }
    
    private void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("enemy"))
        {
            if (area.HasMethod("TakeDamage"))
            {
                area.Call("TakeDamage", Damage);
            }
            QueueFree();
        }
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("enemy"))
        {
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", Damage);
            }
            QueueFree();
        }
    }
}