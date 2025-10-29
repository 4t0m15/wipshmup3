using Godot;

public partial class PlayerBullet : Area2D
{
	[Export] public float Speed { get; set; } =500f;
	[Export] public int Damage { get; set; } =1;

	private Vector2 _screenSize;

	public override void _Ready()
	{
		_screenSize = GetViewport().GetVisibleRect().Size;
		AddToGroup("player_bullets");
		
		// Set up collision detection
		AreaEntered += OnAreaEntered;
	}

	public override void _Process(double delta)
	{
		Position += Vector2.Right * Speed * (float)delta;
		
		// Clean up when off-screen
		if (Position.X > _screenSize.X + 50f)
		{
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		GD.Print($"PlayerBullet: Collision detected with {area.Name}");
		
		// Check if it's an enemy
		if (area.IsInGroup("enemies"))
		{
			GD.Print("PlayerBullet: Hit an enemy! Dealing damage.");
			// Deal damage to enemy
			if (area.HasMethod("TakeDamage"))
			{
				area.Call("TakeDamage", Damage);
			}
			
			// Destroy bullet
			QueueFree();
		}
	}
}
