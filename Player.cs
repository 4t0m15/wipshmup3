using Godot;

public partial class Player : Area2D
{
	[Export] public float MaxSpeed { get; set; } =450f;
	[Export] public float Acceleration { get; set; } =2000f;
	[Export] public float Drag { get; set; } =1200f; // deceleration when no input
	[Export] public float AnimationSpeed { get; set; } =8f; // frames per second

	// Exports added to match scene properties
	[Export] public PackedScene? BulletScene { get; set; } = null;
	[Export] public float FireRate { get; set; } =0.3f;

	private Vector2 _screenSize;
	private AnimatedSprite2D? _anim;
	private Sprite2D? _spriteFallback;

	// Resolved animation names
	private string _idleAnim = string.Empty;
	private string _runAnim = string.Empty;

	// Manual animation state
	private string _manualAnim = string.Empty;
	private int _manualFrame =0;
	private int _manualFrameCount =0;
	private float _manualTimer =0f;

	private Vector2 _velocity = Vector2.Zero;

	// Shooting timer
	private float _fireTimer =0f;

	// Damage and invincibility
	private bool _isInvincible = false;
	private float _invincibilityTime = 1.0f;
	private float _invincibilityTimer = 0f;
	private ColorRect? _flashEffect;

	public override void _Ready()
	{
		// Use the viewport visible rect to get a valid screen size at runtime
		_screenSize = GetViewport().GetVisibleRect().Size;
		AddToGroup("player");

		// Set up collision detection
		AreaEntered += OnAreaEntered;

		// Try to find an AnimatedSprite2D child; if not present, use a Sprite2D fallback
		_anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_spriteFallback = GetNodeOrNull<Sprite2D>("Sprite2D");

		if (_anim != null && _anim.SpriteFrames != null)
		{
			var names = _anim.SpriteFrames.GetAnimationNames();
			// Prefer common animation names
			_runAnim = FindPreferred(names, new string[] { "run", "walk", "move" });
			_idleAnim = FindPreferred(names, new string[] { "idle", "stand", "default", "rest" });

			// Fallbacks
			if (string.IsNullOrEmpty(_runAnim) && names.Length >0)
				_runAnim = names[0];
			if (string.IsNullOrEmpty(_idleAnim) && names.Length >0)
				_idleAnim = names[0];

			// Configure animation speed scale to0 so we control frames manually
			_anim.SpeedScale =0f;
			// Initialize manual animation to idle
			if (!string.IsNullOrEmpty(_idleAnim))
				SetManualAnimation(_idleAnim);
		}
	}

	public override void _Process(double delta)
	{
		// In case the viewport wasn't ready in _Ready, ensure we have a valid size
		if (_screenSize == Vector2.Zero)
			_screenSize = GetViewport().GetVisibleRect().Size;

		// Handle invincibility timer
		if (_isInvincible)
		{
			_invincibilityTimer -= (float)delta;
			if (_invincibilityTimer <= 0f)
			{
				_isInvincible = false;
				// Remove flash effect
				if (_flashEffect != null)
				{
					_flashEffect.QueueFree();
					_flashEffect = null;
				}
			}
		}

		// Get input vector (-1..1)
		var input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		// Apply acceleration
		if (input.Length() >0.1f)
		{
			var dir = input.Normalized();
			_velocity += dir * Acceleration * (float)delta;
			// Cap speed
			if (_velocity.Length() > MaxSpeed)
				_velocity = _velocity.Normalized() * MaxSpeed;
		}
		else
		{
			// Apply drag to slow down smoothly
			var drop = Drag * (float)delta;
			var speed = _velocity.Length();
			if (speed <= drop)
				_velocity = Vector2.Zero;
			else
				_velocity -= _velocity.Normalized() * drop;
		}

		// Move by velocity
		Position += _velocity * (float)delta;

		// Decide target animation based on velocity
		string targetAnim = string.Empty;
		if (_velocity.Length() >10f)
			targetAnim = !string.IsNullOrEmpty(_runAnim) ? _runAnim : _idleAnim;
		else
			targetAnim = _idleAnim;

		// Switch manual animation if needed
		if (_anim != null && !string.IsNullOrEmpty(targetAnim) && _manualAnim != targetAnim)
		{
			SetManualAnimation(targetAnim);
		}

		// Manual frame stepping
		if (_anim != null && _manualFrameCount >0)
		{
			// Advance timer
			_manualTimer += (float)delta;
			float interval = AnimationSpeed >0f ?1f / AnimationSpeed :0.125f;
			if (_manualTimer >= interval)
			{
				// Advance frame and wrap
				_manualTimer -= interval;
				_manualFrame = (_manualFrame +1) % _manualFrameCount;
				_anim.Frame = _manualFrame;
			}

			// Flip sprite based on horizontal velocity (no flip needed for horizontal shooting)
			// Keep sprite facing right for horizontal shooting
		}
		else if (_spriteFallback != null)
		{
			// Keep sprite facing right for horizontal shooting
		}

		// Keep player inside the viewport
		Position = Position.Clamp(Vector2.Zero, _screenSize);

		// Shooting: require Space (ui_accept) to fire, rate-limited by FireRate
		if (BulletScene != null && FireRate >0f)
		{
			if (Input.IsActionPressed("ui_accept"))
			{
				_fireTimer -= (float)delta;
				if (_fireTimer <=0f)
				{
					_fireTimer = FireRate;
					// Instantiate bullet safely
					var inst = BulletScene.Instantiate();
					if (inst is Node2D node)
					{
						node.Position = GlobalPosition + new Vector2(20, 0);
						// Add to scene root so bullets aren't children of player collision
						var root = GetTree().CurrentScene;
						if (root != null)
							root.AddChild(node);
						else
							GetParent()?.AddChild(node);
					}
				}
			}
			else
			{
				_fireTimer = 0f;
			}
		}
	}

	private void SetManualAnimation(string animName)
	{
		if (_anim == null || _anim.SpriteFrames == null) return;
		_manualAnim = animName;
		_manualFrame =0;
		_manualTimer =0f;
		// Ensure animation exists and get frame count
		try
		{
			_manualFrameCount = _anim.SpriteFrames.GetFrameCount(animName);
		}
		catch
		{
			_manualFrameCount =0;
		}
		// Set frame to first frame
		if (_manualFrameCount >0)
			_anim.Frame =0;
	}

	private static string FindPreferred(string[] names, string[] preferred)
	{
		if (names == null || names.Length ==0) return string.Empty;
		// Lowercase lookup
		var lower = new System.Collections.Generic.Dictionary<string, string>();
		foreach (var n in names)
			lower[n.ToLowerInvariant()] = n;
		foreach (var p in preferred)
		{
			if (lower.TryGetValue(p.ToLowerInvariant(), out var actual))
				return actual;
		}
		return string.Empty;
	}

	private void OnAreaEntered(Area2D area)
	{
		// Check if it's an enemy bullet or enemy
		if (area.IsInGroup("enemy_bullets") || area.IsInGroup("enemies"))
		{
			if (!_isInvincible)
			{
				TakeDamage();
			}
		}
	}

	private void TakeDamage()
	{
		// Start invincibility
		_isInvincible = true;
		_invincibilityTimer = _invincibilityTime;

		// Create flash effect
		_flashEffect = new ColorRect();
		_flashEffect.Color = new Color(1, 0, 0, 0.3f); // Red flash
		_flashEffect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		AddChild(_flashEffect);

		// Notify GameManager (autoload)
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		gameManager?.OnPlayerHit();
	}
}
