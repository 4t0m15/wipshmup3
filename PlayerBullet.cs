using Godot;

public partial class PlayerBullet : Area2D
{
	[Export] public float Speed { get; set; } =500f;
	[Export] public int Damage { get; set; } =1;

	public override void _Ready()
	{
		// placeholder
	}

	public override void _Process(double delta)
	{
		Position += Vector2.Up * Speed * (float)delta;
	}
}
