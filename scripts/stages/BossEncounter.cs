using Godot;
using System;

/// <summary>
/// Boss Encounter - Defines a boss battle configuration
/// 
/// This resource class describes a boss encounter within a stage. It's separate
/// from BossTemplate (which defines boss stats) because the same boss could
/// appear in different contexts with different behavior.
/// 
/// Boss: "Ultra-Zeppelin"
///   - A larger version of the Fan enemy
///   - Shoots in a spiral
/// 
/// Boss Design Elements:
///   - Health Pool: Much higher than regular enemies (100 vs 1)
///   - Attack Phases: Changes pattern at health thresholds
///   - Movement Pattern: Typically more complex than regular enemies
///   - Visual Distinction: Larger sprite, more dramatic effects
///   - Audio: Unique music theme during battle
/// 
/// Usage:
///   - Created as .tres resource file (Boss_UltraZeppelin.tres)
///   - Referenced by StageDefinition
///   - Instantiated by StageController at stage end
///   - Managed by boss-specific AI controller
/// 
/// This data-driven approach allows designers to configure boss encounters
/// without touching code, and enables boss reuse across different stages
/// with varying parameters.
/// </summary>
public partial class BossEncounter : Resource
{
	/// <summary>Boss template defining base stats (health, speed, points, etc.)</summary>
	[Export] public BossTemplate BossTemplate { get; set; }
	
	/// <summary>Boss spawn position - typically center-top of screen for dramatic entrance</summary>
	[Export] public Vector2 SpawnPosition { get; set; } = new Vector2(960, 100);
	
	/// <summary>Entrance duration in seconds - boss enters with animation/movement</summary>
	[Export] public float EntranceDuration { get; set; } = 3.0f;
	
	/// <summary>Whether boss is invulnerable during entrance sequence</summary>
	[Export] public bool InvulnerableDuringEntrance { get; set; } = true;
	
	/// <summary>Warning message displayed before boss spawns ("WARNING", "BOSS INCOMING", etc.)</summary>
	[Export] public string WarningMessage { get; set; } = "WARNING";
	
	/// <summary>Boss-specific music track path - overrides stage music</summary>
	[Export] public string BossMusicPath { get; set; } = "";
	
	/// <summary>Attack phase configurations - boss changes pattern at health thresholds</summary>
	[Export] public Godot.Collections.Array<BossPhase> AttackPhases { get; set; } = new();
}

/// <summary>
/// Boss Phase - Defines boss behavior at specific health ranges
/// 
/// Many shmup bosses have multiple attack phases that activate as they take damage.
/// This creates dynamic battles where the boss becomes more dangerous or changes
/// tactics as it gets closer to defeat.
/// 
/// Classic Phase Design:
///   - Phase 1 (100%-75% HP): Basic attack pattern, teaching phase
///   - Phase 2 (75%-50% HP): Intensified pattern, faster bullets
///   - Phase 3 (50%-25% HP): New attack pattern, adds movement
///   - Phase 4 (25%-0% HP): Desperate/berserk mode, fastest/densest pattern
/// 
/// This creates escalating tension and rewards players who can quickly damage
/// the boss before it enters more dangerous phases. It also adds memorization
/// challenge for score attack players.
/// </summary>
public partial class BossPhase : Resource
{
	/// <summary>Phase display name for debugging ("Phase 1: Assault", "Phase 2: Rage", etc.)</summary>
	[Export] public string PhaseName { get; set; } = "Phase 1";
	
	/// <summary>Health percentage when this phase activates (1.0 = 100%, 0.5 = 50%, etc.)</summary>
	[Export] public float HealthThreshold { get; set; } = 1.0f;
	
	/// <summary>Attack pattern override for this phase (replaces base pattern)</summary>
	[Export] public string AttackPattern { get; set; } = "Spiral";
	
	/// <summary>Fire rate multiplier for this phase (1.5 = 50% faster, 2.0 = twice as fast)</summary>
	[Export] public float FireRateMultiplier { get; set; } = 1.0f;
	
	/// <summary>Bullet speed multiplier for this phase (1.2 = 20% faster bullets)</summary>
	[Export] public float BulletSpeedMultiplier { get; set; } = 1.0f;
	
	/// <summary>Movement pattern override for this phase (changes boss movement behavior)</summary>
	[Export] public string MovementPattern { get; set; } = "";
	
	/// <summary>Special effects trigger for this phase ("flash", "shake", "particle_burst", etc.)</summary>
	[Export] public string PhaseTransitionEffect { get; set; } = "";
}

/// <summary>
/// Boss Controller - Runtime AI controller for boss encounters
/// 
/// This is the active system that executes a BossEncounter during gameplay.
/// While BossEncounter is data (what the boss is), BossController is behavior
/// (how the boss acts).
/// 
/// Responsibilities:
///   - Execute boss entrance sequence
///   - Manage phase transitions based on health
///   - Control boss movement and attack patterns
///   - Handle boss defeat and death sequence
///   - Emit signals for boss events
/// 
/// Signal Flow (from shmupbible):
///   Boss.defeated -> StageController.boss_defeated -> HUD Popup
///   Boss.defeated -> CombatSystem.OnBossDefeated() -> Award points
///   Boss.phase_changed -> Visual/audio effects
/// 
/// This controller brings together the boss template data, encounter configuration,
/// and actual runtime logic to create the complete boss battle experience.
/// </summary>
public partial class BossController : Area2D
{
	/// <summary>The encounter configuration being executed</summary>
	[Export] public BossEncounter Encounter { get; set; }
	
	/// <summary>Current boss health - tracked separately from template for phase management</summary>
	private int _currentHealth;
	
	/// <summary>Maximum boss health - cached from template on initialization</summary>
	private int _maxHealth;
	
	/// <summary>Current active phase index</summary>
	private int _currentPhaseIndex = 0;
	
	/// <summary>Whether boss is in entrance sequence (invulnerable)</summary>
	private bool _isEntering = true;
	
	/// <summary>Time elapsed during entrance sequence</summary>
	private float _entranceTime = 0f;
	
	/// <summary>Bullet scene prefab for spawning boss bullets</summary>
	[Export] public PackedScene BulletScene { get; set; }
	
	/// <summary>Current attack and movement behaviors (loaded from phase)</summary>
	private AttackBehavior _attackBehavior;
	private MovementBehavior _movementBehavior;
	
	// Signals
	/// <summary>Emitted when boss entrance sequence completes</summary>
	[Signal] public delegate void EntranceCompleteEventHandler();
	
	/// <summary>Emitted when boss transitions to new phase</summary>
	[Signal] public delegate void PhaseChangedEventHandler(string phaseName);
	
	/// <summary>Emitted when boss is defeated</summary>
	[Signal] public delegate void BossDefeatedEventHandler(int points);
	
	/// <summary>
	/// Initialize boss controller
	/// 
	/// Setup Sequence:
	/// 1. Load boss template stats
	/// 2. Set initial health and max health
	/// 3. Position boss at spawn point
	/// 4. Configure collision layers (ENEMY)
	/// 5. Load initial phase (Phase 0 or first phase)
	/// 6. Begin entrance sequence
	/// 7. Start boss music
	/// </summary>
	public override void _Ready()
	{
		if (Encounter?.BossTemplate == null)
		{
			GD.PrintErr("BossController: No encounter or template assigned!");
			return;
		}
		
		_maxHealth = Encounter.BossTemplate.Health;
		_currentHealth = _maxHealth;
		Position = Encounter.SpawnPosition;
		
		// Set collision
		CollisionLayer = 8; // Enemy layer (same as regular enemies)
		CollisionMask = 2; // Player bullet layer
		AddToGroup("enemy");
		AddToGroup("boss");
		
		LoadPhase(0);
		StartEntrance();
		
		GD.Print($"Boss spawned: {Encounter.BossTemplate.BossName} (HP: {_currentHealth})");
	}
	
	/// <summary>
	/// Begin boss entrance sequence
	/// 
	/// Entrance creates dramatic tension before battle starts:
	/// - Boss moves from spawn position to battle position
	/// - Background music transitions to boss theme
	/// - Boss is typically invulnerable during entrance
	/// - Visual effects (screen shake, flash, particles)
	/// 
	/// Duration is controlled by Encounter.EntranceDuration (typically 3 seconds).
	/// </summary>
	private void StartEntrance()
	{
		_isEntering = true;
		_entranceTime = 0f;
		
		// Future: Play entrance animation
		// Future: Transition to boss music
		// Future: Display boss name popup
	}
	
	/// <summary>
	/// Load and activate a boss phase
	/// 
	/// Phase Loading:
	/// 1. Get phase configuration from encounter
	/// 2. Create/update attack behavior with phase multipliers
	/// 3. Create/update movement behavior if phase specifies
	/// 4. Emit phase changed signal for effects
	/// 
	/// Phase transitions are dramatic moments that signal to the player
	/// that they've made progress but the boss is getting desperate.
	/// </summary>
	private void LoadPhase(int phaseIndex)
	{
		if (phaseIndex >= Encounter.AttackPhases.Count)
		{
			GD.Print("No more phases - using base behavior");
			return;
		}
		
		var phase = Encounter.AttackPhases[phaseIndex];
		_currentPhaseIndex = phaseIndex;
		
		EmitSignal(SignalName.PhaseChanged, phase.PhaseName);
		GD.Print($"Boss Phase: {phase.PhaseName}");
		
		// Future: Apply phase configuration to behaviors
		// _attackBehavior.FireRate = baseFireRate / phase.FireRateMultiplier;
		// _attackBehavior.BulletSpeed = baseBulletSpeed * phase.BulletSpeedMultiplier;
	}
	
	/// <summary>
	/// Update boss behavior each frame
	/// 
	/// Update Sequence:
	/// 1. If entering: Advance entrance timer, check for completion
	/// 2. If battle active: Update movement and attack behaviors
	/// 3. Check for phase transitions based on current health percentage
	/// 
	/// Boss behavior is similar to regular enemies but with phase management.
	/// </summary>
	public override void _Process(double delta)
	{
		float dt = (float)delta;
		
		// Handle entrance sequence
		if (_isEntering)
		{
			_entranceTime += dt;
			if (_entranceTime >= Encounter.EntranceDuration)
			{
				_isEntering = false;
				EmitSignal(SignalName.EntranceComplete);
				GD.Print("Boss entrance complete - FIGHT!");
			}
			return;
		}
		
		// Update behaviors (similar to regular enemies)
		if (_movementBehavior != null)
		{
			Position += _movementBehavior.CalculateMovement(dt, Position);
		}
		
		if (_attackBehavior != null)
		{
			_attackBehavior.Update(dt);
			if (_attackBehavior.CanFire())
			{
				Shoot();
			}
		}
		
		// Check for phase transitions
		CheckPhaseTransition();
	}
	
	/// <summary>
	/// Check if boss should transition to next phase
	/// 
	/// Phase Trigger:
	/// - Calculate current health percentage
	/// - Check if crossed next phase's threshold
	/// - If yes: Load next phase (with transition effects)
	/// 
	/// Example: Boss at 45% HP crosses 50% threshold, triggers Phase 3
	/// </summary>
	private void CheckPhaseTransition()
	{
		if (_currentPhaseIndex + 1 >= Encounter.AttackPhases.Count) return;
		
		float healthPercent = (float)_currentHealth / _maxHealth;
		var nextPhase = Encounter.AttackPhases[_currentPhaseIndex + 1];
		
		if (healthPercent <= nextPhase.HealthThreshold)
		{
			LoadPhase(_currentPhaseIndex + 1);
			// Future: Play transition effects (flash, shake, etc.)
		}
	}
	
	/// <summary>
	/// Spawn boss bullet pattern
	/// 
	/// Same as regular enemy shooting but typically more complex patterns.
	/// Boss attack behaviors return larger arrays of directions for denser patterns.
	/// </summary>
	private void Shoot()
	{
		// Implementation same as Enemy.Shoot() - spawn bullets in pattern
		// Future: Implement when bullet spawning is active
	}
	
	/// <summary>
	/// Handle boss taking damage from player bullets
	/// 
	/// Should be connected to AreaEntered signal.
	/// 
	/// Damage Sequence:
	/// 1. Check if in entrance (ignore damage if invulnerable)
	/// 2. Reduce health
	/// 3. Check for death (health <= 0)
	/// 4. If dead: Trigger defeat sequence
	/// 
	/// Boss defeat is a major victory moment deserving celebration.
	/// </summary>
	private void OnDamage(int damage)
	{
		if (_isEntering && Encounter.InvulnerableDuringEntrance) return;
		
		_currentHealth -= damage;
		GD.Print($"Boss HP: {_currentHealth}/{_maxHealth}");
		
		if (_currentHealth <= 0)
		{
			OnDefeated();
		}
	}
	
	/// <summary>
	/// Execute boss defeat sequence
	/// 
	/// Victory Sequence:
	/// 1. Emit defeated signal with point value
	/// 2. Award massive point bonus through GameState
	/// 3. Trigger explosion sequence (multiple explosions over time)
	/// 4. Display victory popup
	/// 5. Stop boss music, return to stage clear music
	/// 6. Clean up boss node
	/// 
	/// This is a climactic moment that should feel rewarding and dramatic.
	/// The multi-explosion sequence is classic shmup boss death choreography.
	/// </summary>
	private void OnDefeated()
	{
		EmitSignal(SignalName.BossDefeated, Encounter.BossTemplate.PointValue);
		
		var gameState = GetNodeOrNull<GameState>("/root/GameState");
		gameState?.AddScore(Encounter.BossTemplate.PointValue);
		
		GD.Print($"BOSS DEFEATED: {Encounter.BossTemplate.BossName}!");
		
		// Future: Multi-explosion death sequence
		// Future: Victory popup and stage clear
		
		QueueFree();
	}
}
