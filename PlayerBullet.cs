using Godot;

public partial class PlayerBullet : Area2D
{
	[Export] public float Speed { get; set; } = 500f;
	private Vector2 _direction = Vector2.Right;

	public override void _Ready()
	{
		AddToGroup("player_bullet");
		AreaEntered += OnAreaEntered;
		GetTree().CreateTimer(3.0).Timeout += QueueFree;
	}

	public override void _Process(double delta)
	{
		Position += _direction * Speed * (float)delta;
		if (Position.X > 1200) QueueFree();
	}

	public void SetDirection(Vector2 direction) => _direction = direction.Normalized();

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("enemy"))
		{
			if (area.HasMethod("TakeDamage")) area.Call("TakeDamage", 1);
			QueueFree();
		}
	}
}