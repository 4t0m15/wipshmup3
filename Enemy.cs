using Godot;

public partial class Enemy : Area2D
{
	[Export] public float Speed { get; set; } = 200f;
	[Export] public int Health { get; set; } = 3;
	[Export] public int ScoreValue { get; set; } = 100;

	private UI? _ui;

	public override void _Ready()
	{
		AddToGroup("enemy");
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
		_ui = GetTree().GetFirstNodeInGroup("ui") as UI;
	}

	public override void _Process(double delta)
	{
		Position += Vector2.Left * Speed * (float)delta;
		if (Position.X < -100) QueueFree();
	}

	public void Initialize(Vector2 startPosition, float speed = -1f)
	{
		Position = startPosition;
		if (speed > 0) Speed = speed;
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("player_bullet"))
		{
			Health--;
			area.QueueFree();
			if (Health <= 0)
			{
				Die();
			}
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player"))
		{
			if (body.HasMethod("TakeDamage")) body.Call("TakeDamage", 1);
			Die();
		}
	}

	public void TakeDamage(int damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		_ui?.AddScore(ScoreValue);
		
		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (sprite != null)
		{
			sprite.Modulate = Colors.Red;
		}
		else
		{
			var animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			if (animSprite != null)
			{
				animSprite.Modulate = Colors.Red;
			}
		}
		
		QueueFree();
	}
}