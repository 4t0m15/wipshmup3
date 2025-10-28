using Godot;

public partial class EnemyBullet : Area2D
{
	[Export] public float Speed { get; set; } =300f;
	[Export] public int Damage { get; set; } =1;

	public override void _Ready()
	{
		// placeholder
	}

	public override void _Process(double delta)
	{
		Position += Vector2.Down * Speed * (float)delta;
	}
}
