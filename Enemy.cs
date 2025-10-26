using Godot;

public partial class Enemy : Area2D
{
    [Export] public float Speed { get; set; } = 200f;
    [Export] public int Health { get; set; } = 3;
    [Export] public int ScoreValue { get; set; } = 100;
    [Export] public float FireRate { get; set; } = 1.5f;
    [Export] public PackedScene BulletScene { get; set; }
    
    private Vector2 _screenSize;
    private Timer _fireTimer;
    private ColorRect _sprite;
    private CollisionShape2D _collisionShape;
    private bool _isDead = false;
    
    public override void _Ready()
    {
        _screenSize = GetViewportRect().Size;
        _sprite = GetNode<ColorRect>("ColorRect");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        
        // Set up firing timer
        _fireTimer = new Timer();
        _fireTimer.WaitTime = FireRate;
        _fireTimer.Timeout += OnFireTimerTimeout;
        _fireTimer.Autostart = true;
        AddChild(_fireTimer);
        
        // Connect signals
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
        
        // Connect screen exit signal
        var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        notifier.ScreenExited += OnVisibleOnScreenNotifier2DScreenExited;
        
        // Add to enemy group
        AddToGroup("enemy");
    }
    
    public override void _Process(double delta)
    {
        if (_isDead) return;
        
        // Move left across the screen
        Position += Vector2.Left * Speed * (float)delta;
        
        // Remove if off screen
        if (Position.X < -100)
        {
            QueueFree();
        }
    }
    
    public void Initialize(Vector2 startPosition, float speed = -1f)
    {
        Position = startPosition;
        if (speed > 0)
            Speed = speed;
    }
    
    private void OnFireTimerTimeout()
    {
        if (_isDead || BulletScene == null) return;
        
        // Create bullet
        var bullet = BulletScene.Instantiate<Area2D>();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.Position = Position + Vector2.Left * 20;
        
        // Set bullet direction (leftward toward player)
        if (bullet.HasMethod("SetDirection"))
        {
            bullet.Call("SetDirection", Vector2.Left);
        }
    }
    
    private void OnAreaEntered(Area2D area)
    {
        if (_isDead) return;
        
        // Check if it's a player bullet
        if (area.IsInGroup("player_bullet"))
        {
            TakeDamage(1);
            area.QueueFree(); // Remove the bullet
        }
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (_isDead) return;
        
        // Check if it's the player
        if (body.IsInGroup("player"))
        {
            // Damage player and destroy enemy
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", 1);
            }
            Die();
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        
        Health -= damage;
        
        // Flash effect
        if (_sprite != null)
        {
            _sprite.Color = Colors.Red;
            GetTree().CreateTimer(0.1).Timeout += () => {
                if (_sprite != null)
                    _sprite.Color = new Color(0.8f, 0.2f, 0.2f, 1.0f);
            };
        }
        
        if (Health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        if (_isDead) return;
        
        _isDead = true;
        
        // Add explosion effect
        if (_sprite != null)
        {
            _sprite.Color = Colors.Orange;
            _sprite.Scale = Vector2.One * 1.2f;
        }
        
        // Disable collision
        if (_collisionShape != null)
            _collisionShape.SetDeferred("disabled", true);
        
        // Add score
        if (GetTree().HasGroup("game_manager"))
        {
            var gameManager = GetTree().GetFirstNodeInGroup("game_manager");
            if (gameManager.HasMethod("AddScore"))
            {
                gameManager.Call("AddScore", ScoreValue);
            }
        }
        
        // Remove after short delay
        GetTree().CreateTimer(0.2).Timeout += () => QueueFree();
    }
    
    private void OnVisibleOnScreenNotifier2DScreenExited()
    {
        QueueFree();
    }
}