using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Stage Definition - Data-driven stage configuration for Story Mode
/// 
/// This resource class defines the structure and progression of a stage in
/// the shmup's story mode. It's part of the data-driven design philosophy
/// from the shmupbible where game content is authored in resources rather
/// than hardcoded in scripts.
/// 
/// Stage Structure (classic shmup design):
///   1. Opening: Player enters, music starts, HUD shows stage title
///   2. Wave Phase: Series of enemy waves with varying patterns
///   3. Mid-Boss: Optional mini-boss encounter for difficulty spike
///   4. Final Wave: Intense enemy assault before boss
///   5. Boss Encounter: Stage-ending boss battle
///   6. Clear Screen: Victory popup, bonus calculation
///   7. Transition: Brief intermission before next stage
/// 
/// Usage Pattern:
///   - Create .tres resource files for each stage (Stage1.tres, Stage2.tres, etc.)
///   - Configure enemy waves, spawn patterns, boss encounter
///   - StageController loads and executes the stage definition
/// 
/// This separation of stage data from stage logic allows designers to create
/// new stages without programming, and enables modding/user content.
/// </summary>
public partial class StageDefinition : Resource
{
	/// <summary>Stage number for display (1, 2, 3, etc.)</summary>
	[Export] public int StageNumber { get; set; } = 1;
	
	/// <summary>Stage title shown during opening (e.g., "Aerial Assault", "Ocean Fortress")</summary>
	[Export] public string StageName { get; set; } = "Stage 1";
	
	/// <summary>Background music resource path for this stage</summary>
	[Export] public string MusicPath { get; set; } = "";
	
	/// <summary>Background image/tilemap for this stage's aesthetic</summary>
	[Export] public string BackgroundPath { get; set; } = "";
	
	/// <summary>Stage duration in seconds before forced boss encounter</summary>
	[Export] public float StageDuration { get; set; } = 120f;
	
	/// <summary>Enemy wave configurations - defines spawn patterns throughout stage</summary>
	[Export] public Godot.Collections.Array<EnemyWave> EnemyWaves { get; set; } = new();
	
	/// <summary>Boss encounter that ends this stage</summary>
	[Export] public BossEncounter BossEncounter { get; set; }
	
	/// <summary>Base score bonus for clearing this stage (before time bonus)</summary>
	[Export] public int StageClearBonus { get; set; } = 5000;
}

/// <summary>
/// Enemy Wave - Defines a group of enemies that spawn together
/// 
/// Waves are the building blocks of stage design. A stage consists of multiple
/// waves that create rhythm and pacing. Classic examples:
///   - Opening wave: 3-5 weak enemies to warm up player
///   - V-Formation: 5 enemies in V shape, Type 01 pattern
///   - Flanking wave: Enemies from both sides simultaneously
///   - Bullet hell wave: Type 02 Fan enemies for intense dodging
/// 
/// Wave Design Philosophy:
///   - Rhythm: Alternate intense and calm waves for pacing
///   - Teaching: Early waves introduce patterns that later waves combine
///   - Fairness: Give player breathing room between difficult waves
///   - Escalation: Each wave should be slightly harder than previous
/// </summary>
public partial class EnemyWave : Resource
{
	/// <summary>Time in seconds when this wave spawns (from stage start)</summary>
	[Export] public float SpawnTime { get; set; } = 0f;
	
	/// <summary>Enemy template to spawn in this wave (references EnemyTemplate resource)</summary>
	[Export] public EnemyTemplate EnemyTemplate { get; set; }
	
	/// <summary>Number of enemies to spawn in this wave</summary>
	[Export] public int EnemyCount { get; set; } = 5;
	
	/// <summary>Formation pattern for this wave: "Line", "V", "Arc", "Scattered", "Sides"</summary>
	[Export] public string FormationPattern { get; set; } = "Line";
	
	/// <summary>Time between individual enemy spawns within the wave (0 = instant, >0 = staggered)</summary>
	[Export] public float SpawnInterval { get; set; } = 0.2f;
	
	/// <summary>Horizontal spawn area multiplier (1.0 = full screen width, 0.5 = center half, etc.)</summary>
	[Export] public float SpawnWidth { get; set; } = 1.0f;
}

/// <summary>
/// Stage Controller - Executes a StageDefinition's content
/// 
/// This is the runtime system that takes a StageDefinition resource and
/// brings it to life during gameplay. It's the director that orchestrates
/// enemy spawns, music, backgrounds, and boss transitions.
/// 
/// Responsibilities:
///   - Load and parse stage definition
///   - Spawn enemy waves at specified times
///   - Track stage progress and time remaining
///   - Trigger boss encounter when stage duration reached
///   - Calculate and award stage clear bonuses
///   - Emit signals for stage events (started, wave spawn, boss warning, cleared)
/// 
/// Signal Flow (from shmupbible):
///   StageController.enemy_killed -> Update kill counter
///   StageController.boss_defeated -> Stage clear sequence
///   StageController -> HUD.display_stage_title()
///   StageController -> AudioManager.play_stage_music()
/// 
/// This controller is the bridge between static stage data and dynamic gameplay.
/// </summary>
public partial class StageController : Node
{
	/// <summary>The stage definition being executed</summary>
	[Export] public StageDefinition CurrentStage { get; set; }
	
	/// <summary>Time elapsed since stage started</summary>
	private float _stageTime = 0f;
	
	/// <summary>Index of next wave to spawn (tracks wave progression)</summary>
	private int _nextWaveIndex = 0;
	
	/// <summary>Whether the boss has been spawned yet</summary>
	private bool _bossSpawned = false;
	
	/// <summary>Reference to main scene for spawning enemies</summary>
	private Node2D _mainScene;
	
	// Signals for stage events
	/// <summary>Emitted when stage begins - triggers title display, music, etc.</summary>
	[Signal] public delegate void StageStartedEventHandler(string stageName);
	
	/// <summary>Emitted when enemy wave spawns - for debugging and HUD updates</summary>
	[Signal] public delegate void WaveSpawnedEventHandler(int waveNumber);
	
	/// <summary>Emitted shortly before boss spawns - warning to player</summary>
	[Signal] public delegate void BossWarningEventHandler();
	
	/// <summary>Emitted when boss is defeated and stage is cleared</summary>
	[Signal] public delegate void StageClearedEventHandler(int bonus);
	
	/// <summary>
	/// Initialize stage controller
	/// 
	/// Setup:
	/// - Get reference to main scene for enemy spawning
	/// - Validate stage definition exists
	/// - Reset timers and state
	/// </summary>
	public override void _Ready()
	{
		_mainScene = GetParent() as Node2D;
		
		if (CurrentStage != null)
		{
			StartStage();
		}
	}
	
	/// <summary>
	/// Begin stage execution
	/// 
	/// Sequence:
	/// 1. Reset stage timer and wave index
	/// 2. Emit StageStarted signal for HUD/music
	/// 3. Display stage title popup
	/// 4. Start stage music
	/// 5. Begin wave spawning logic
	/// </summary>
	public void StartStage()
	{
		_stageTime = 0f;
		_nextWaveIndex = 0;
		_bossSpawned = false;
		
		EmitSignal(SignalName.StageStarted, CurrentStage.StageName);
		GD.Print($"=== {CurrentStage.StageName} START ===");
		
		// Future: Load background, start music, show title
	}
	
	/// <summary>
	/// Update stage progression each frame
	/// 
	/// Responsibilities:
	/// 1. Advance stage timer
	/// 2. Check and spawn due enemy waves
	/// 3. Check if boss spawn time reached
	/// 4. Spawn boss encounter when ready
	/// 
	/// Wave Spawning Logic:
	///   - Iterate through wave definitions in order
	///   - When stage time >= wave.SpawnTime, spawn that wave
	///   - Mark wave as spawned (increment index)
	///   - Continue until all waves spawned
	/// 
	/// Boss Spawning Logic:
	///   - When stage time >= StageDuration
	///   - Emit boss warning (dramatic music change, warning popup)
	///   - Wait 3 seconds for drama
	///   - Spawn boss encounter
	///   - Mark boss as spawned (prevents re-spawning)
	/// </summary>
	public override void _Process(double delta)
	{
		if (CurrentStage == null || _bossSpawned) return;
		
		_stageTime += (float)delta;
		
		// Spawn due enemy waves
		while (_nextWaveIndex < CurrentStage.EnemyWaves.Count)
		{
			var wave = CurrentStage.EnemyWaves[_nextWaveIndex];
			if (_stageTime >= wave.SpawnTime)
			{
				SpawnWave(wave, _nextWaveIndex);
				_nextWaveIndex++;
			}
			else
			{
				break; // Waves are ordered, so we can stop checking
			}
		}
		
		// Check for boss spawn
		if (_stageTime >= CurrentStage.StageDuration && !_bossSpawned)
		{
			SpawnBoss();
		}
	}
	
	/// <summary>
	/// Spawn an enemy wave according to its configuration
	/// 
	/// Process:
	/// 1. Emit wave spawned signal
	/// 2. Calculate spawn positions based on FormationPattern
	/// 3. Spawn enemies at those positions (instantly or staggered)
	/// 4. Apply enemy template properties to each enemy
	/// 
	/// Formation Patterns:
	///   - "Line": Horizontal line across top of screen
	///   - "V": V formation pointing down
	///   - "Arc": Curved arc across top
	///   - "Scattered": Random positions
	///   - "Sides": Two groups from left and right edges
	/// 
	/// If SpawnInterval > 0, enemies spawn sequentially with delay.
	/// If SpawnInterval = 0, entire wave spawns instantly.
	/// </summary>
	private void SpawnWave(EnemyWave wave, int waveNumber)
	{
		EmitSignal(SignalName.WaveSpawned, waveNumber);
		GD.Print($"Spawning Wave {waveNumber}: {wave.EnemyCount} x {wave.EnemyTemplate?.EnemyName}");
		
		// Future: Implement formation patterns and enemy spawning
		// For now, this is a placeholder that shows the architecture
	}
	
	/// <summary>
	/// Spawn the stage's boss encounter
	/// 
	/// Sequence:
	/// 1. Clear remaining regular enemies (optional, depending on design)
	/// 2. Emit boss warning signal (3 second countdown, dramatic music change)
	/// 3. Display "WARNING" popup with boss name
	/// 4. Spawn boss from BossEncounter definition
	/// 5. Mark boss as spawned
	/// 
	/// Boss spawning is a major event that deserves dramatic buildup.
	/// The warning period gives players time to mentally prepare and
	/// position themselves optimally for the fight.
	/// </summary>
	private void SpawnBoss()
	{
		_bossSpawned = true;
		EmitSignal(SignalName.BossWarning);
		GD.Print($"!!! BOSS WARNING: {CurrentStage.BossEncounter?.BossTemplate?.BossName} !!!");
		
		// Future: Wait 3 seconds, then spawn boss
		// Future: Change music to boss theme
		// Future: Display dramatic warning popup
	}
	
	/// <summary>
	/// Handle stage clear when boss is defeated
	/// 
	/// Called by: Boss defeat signal
	/// 
	/// Sequence:
	/// 1. Calculate time bonus (faster clear = more points)
	/// 2. Award stage clear bonus + time bonus
	/// 3. Display stage clear popup with stats
	/// 4. Wait for player input or timeout
	/// 5. Transition to next stage or victory screen
	/// 
	/// Bonus Calculation:
	///   Base: StageClearBonus (e.g., 5000)
	///   Time: Max 5000 points for fast clear, decreases over time
	///   Total: Up to 10000 points for perfect stage clear
	/// 
	/// This rewards skilled play and creates replay value as players
	/// try to optimize their stage clear times.
	/// </summary>
	public void OnBossDefeated()
	{
		// Calculate time bonus
		float timeBonus = Mathf.Max(0, 5000 - (_stageTime * 10));
		int totalBonus = CurrentStage.StageClearBonus + (int)timeBonus;
		
		EmitSignal(SignalName.StageCleared, totalBonus);
		GD.Print($"=== STAGE CLEAR! Bonus: {totalBonus} ===");
		
		// Future: Award bonus points, display clear screen, advance to next stage
	}
}
