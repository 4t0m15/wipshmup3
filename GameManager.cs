using Godot;

public partial class GameManager : Node
{
	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public PackedScene EnemyBulletScene { get; set; }
	[Export] public int PlayerHealth { get; set; } =3;
	[Export] public float SpawnRate { get; set; } =1.0f;

	public override void _Ready()
	{
		// Placeholder GameManager created to satisfy scene resource references.
	}
}
