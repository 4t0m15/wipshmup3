using Godot;

public partial class Player : Area2D
{ 
	[Export] public int Speed { get; set; } = 400;
	public Vector2 ScreenSize;
	//start horizontal
	private bool _preferHorizontal = true;
	public override void _Ready() //launches on boot. this is like arduino
	{
		ScreenSize = GetViewportRect().Size;
	}
	public override void _Process(double delta)
	{
		var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (Input.IsActionJustPressed("ui_up"))  anim.Animation = "up";
		if (Input.IsActionJustPressed("ui_down")) anim.Animation = "down";
		int xDir = (Input.IsActionPressed("move_right") ? 1 : 0) - (Input.IsActionPressed("move_left") ? 1 : 0);
		int yDir = (Input.IsActionPressed("move_down")  ? 1 : 0) - (Input.IsActionPressed("move_up")   ? 1 : 0);
		var velocity = new Vector2(xDir, yDir);
		if (velocity != Vector2.Zero)
		{
			velocity = velocity.Normalized() * Speed;
			anim.Play();
		}
		else anim.Stop();
		if (velocity.Y > 0) anim.Animation = "down";
		else if (velocity.Y < 0) anim.Animation = "up";
		else anim.Animation = "left_right";
		if (velocity.X != 0) anim.FlipV = false;
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
