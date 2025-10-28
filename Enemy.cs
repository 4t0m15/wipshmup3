using Godot;

public partial class Enemy : Area2D
{
 [Export] public float Speed { get; set; } =200f;
 [Export] public int Health { get; set; } =1;
 [Export] public int ScoreValue { get; set; } =100;
 [Export] public float FireRate { get; set; } =1.0f;

 public override void _Ready()
 {
 // placeholder
 }
}
