using Godot;
public partial class Player : Area2D
{
	[Export] public int Speed { get; set; } = 450;
	[Export] public int Health { get; set; } = 3;
	[Export] public PackedScene? BulletScene { get; set; }
	[Export] public float FireRate { get; set; } = 0.3f;

	private Vector2 _screenSize;
	private float _fireTimer;
	private UI? _ui;

	public override void _Ready()
	{
		_screenSize = GetViewportRect().Size;
		AddToGroup("player");
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;

		_ui = GetTree().GetFirstNodeInGroup("ui") as UI;
		_ui?.SetHealth(Health);
	}

	public override void _Process(double delta)
	{
		var input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Position += input * Speed * (float)delta;
		Position = Position.Clamp(Vector2.Zero, _screenSize);

		_fireTimer -= (float)delta;
		if (Input.IsActionPressed("ui_accept") && _fireTimer <= 0)
		{
			_fireTimer = FireRate;
			if (BulletScene != null)
			{
				var bullet = BulletScene.Instantiate<Area2D>();
				GetTree().CurrentScene.AddChild(bullet);
				bullet.Position = Position + Vector2.Right * 30;
			}
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("enemy_bullet"))
		{
			Health--;
			_ui?.SetHealth(Health);
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("enemy"))
		{
			Health--;
			_ui?.SetHealth(Health);
		}
	}

	public void TakeDamage(int damage)
	{
		Health -= damage;
		_ui?.SetHealth(Health);
	}
}
