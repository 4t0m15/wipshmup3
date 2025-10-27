using Godot;

public partial class EnemyBullet : Area2D
{
	[Export] public float Speed { get; set; } = 300f;
	private Vector2 _direction = Vector2.Left;

	public override void _Ready()
	{
		AddToGroup("enemy_bullet");
		AreaEntered += OnAreaEntered;
		GetTree().CreateTimer(3.0).Timeout += QueueFree;
	}

	public override void _Process(double delta)
	{
		Position += _direction * Speed * (float)delta;
		if (Position.X < -50 || Position.X > 1200) QueueFree();
	}

	public void SetDirection(Vector2 direction) => _direction = direction.Normalized();

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("player") && area.HasMethod("TakeDamage"))
			area.Call("TakeDamage", 1);
	}
}