using Godot;

public partial class Player : Area2D
{
	[Export] public int Speed { get; set; } = 400;

	public Vector2 ScreenSize;

	// last-pressed axis wins: true = horizontal, false = vertical
	private bool _preferHorizontal = true;

	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
	}

	public override void _Process(double delta)
	{
		var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		// Track last-pressed axis
		if (Input.IsActionJustPressed("move_left") || Input.IsActionJustPressed("move_right"))
			_preferHorizontal = true;
		// Build direction
		int xDir = (Input.IsActionPressed("move_right") ? 1 : 0) - (Input.IsActionPressed("move_left") ? 1 : 0);
		int yDir = (Input.IsActionPressed("move_down")  ? 1 : 0) - (Input.IsActionPressed("move_up")   ? 1 : 0);

		// If both are held, keep only the last-pressed axis
		if (xDir != 0 && yDir != 0)
		{
			if (_preferHorizontal) yDir = 0;
			else xDir = 0;
		}

		var velocity = new Vector2(xDir, yDir);
		//choose animation so that it chooses which animation it chooses based on key WHILE it is pressed. Its cool now!
		if (velocity.Y > 0) anim.Animation = "down";
		else if (velocity.Y < 0) anim.Animation = "up";
		else if (velocity.X != 0) anim.Animation = "left_right";
		else anim.Animation = "left_right";
		
		if (velocity.X != 0)
		{
			//anim.Animation = "down";      // <<< add this so it switches off "up"
			//anim.FlipH = velocity.X < 0;
			anim.FlipV = false;
		}
		else if (velocity.Y != 0)
		{
			//anim.Animation = "down";
			anim.FlipV = false; // DONT flip for "down"
			anim.FlipH = false;
		}

		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * Speed;
			anim.Play(); 
		}
		else
		{
			anim.Stop();
		}

		Position += velocity * (float)delta;
		Position = new Vector2(
			Mathf.Clamp(Position.X, 0, ScreenSize.X),
			Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);
	}
}
