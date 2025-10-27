using Godot;

public partial class GameManager : Node
{
    [Export] public PackedScene? EnemyScene { get; set; }
    [Export] public float SpawnRate { get; set; } = 2.0f;

    private Timer? _spawnTimer;
    private Vector2 _screenSize;
    private UI? _ui;

    public override void _Ready()
    {
        _screenSize = GetViewport().GetVisibleRect().Size;
        AddToGroup("game_manager");

        // Create UI
        _ui = new UI();
        _ui.AddToGroup("ui");
        AddChild(_ui);

        _spawnTimer = new Timer { WaitTime = SpawnRate, Autostart = true };
        _spawnTimer.Timeout += SpawnEnemy;
        AddChild(_spawnTimer);
    }

    private void SpawnEnemy()
    {
        if (EnemyScene == null) return;
        var enemy = EnemyScene.Instantiate<Enemy>();
        AddChild(enemy);
        float y = GD.Randf() * (_screenSize.Y - 100) + 50;
        float speed = (float)GD.RandRange(140.0, 280.0);
        enemy.Initialize(new Vector2(_screenSize.X + 50, y), speed);
    }
}