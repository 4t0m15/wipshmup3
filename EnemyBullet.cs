using Godot;

public partial class EnemyBullet : Area2D
{
	[Export] public float Speed { get; set; } = 300f;
	[Export] public int Damage { get; set; } = 1;

	private Vector2 _screenSize;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;
		AddToGroup("enemy_bullets");
		
		// Set up collision detection
		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		Position += Vector2.Left * Speed * (float)delta;
		
		// Clean up when off-screen
		if (Position.X < -50f)
		{
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		// Check if it's the player
		if (area.IsInGroup("player"))
		{
			// The player will handle damage, just destroy bullet
			QueueFree();
		}
	}
}
