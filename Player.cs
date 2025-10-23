using Godot;
public partial class Player : Area2D
{ 
	[Export] public int Speed { get; set; } = 250;
	//do what c#; linter says
	private Vector2 _screenSize;
	public override void _Ready() //launches on boot. this is like arduino
	{
		_screenSize = GetViewportRect().Size;
	}
	public override void _Process(double delta)
	{
		//tell game to play animations based on input
		var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (Input.IsActionJustPressed("ui_up"))  anim.Animation = "up";
		if (Input.IsActionJustPressed("ui_down")) anim.Animation = "down";
		//tell game to set directions
		int xDir = (Input.IsActionPressed("move_right") ? 1 : 0) - (Input.IsActionPressed("move_left") ? 1 : 0);
		int yDir = (Input.IsActionPressed("move_down")  ? 1 : 0) - (Input.IsActionPressed("move_up")   ? 1 : 0);
		//set movement speed
		var velocity = new Vector2(xDir, yDir);
		if (velocity != Vector2.Zero) { velocity = velocity.Normalized() * Speed; anim.Play(); }
		else anim.Stop();
		//set animations
		if (velocity.Y > 0) anim.Animation = "down";
		else if (velocity.Y < 0) anim.Animation = "up";
		else anim.Animation = "left_right";
		//set speeds
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
		//stop player from going off screen
		Position = new Vector2(
			Mathf.Clamp(Position.X, 0, _screenSize.X),
			Mathf.Clamp(Position.Y, 0, _screenSize.Y)
		);
	}
}
