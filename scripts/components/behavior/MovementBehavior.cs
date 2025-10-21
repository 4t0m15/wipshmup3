using Godot;
using System;

/// <summary>
/// Base Movement Behavior Component - Pluggable enemy movement patterns
/// 
/// This is the base class for the Component-Based Enemy System described in the
/// shmupbible architecture. Rather than having monolithic enemy classes with
/// hardcoded movement, behaviors are modular components that can be mixed and matched.
/// 
/// Architecture Benefits:
///   - Reusability: Same behavior works for multiple enemy types
///   - Flexibility: Swap behaviors at runtime or through scene configuration
///   - Maintainability: Each behavior is a small, focused class
///   - Testability: Behaviors can be tested independently
/// 
/// Usage Pattern:
///   1. Enemy adds MovementBehavior as child node
///   2. Enemy calls CalculateMovement() each frame
///   3. Behavior returns velocity delta based on its pattern
///   4. Enemy applies the delta to its position
/// 
/// Built-in Implementations:
///   - StraightMovement: Simple downward movement (Type 01 enemies)
///   - SineWaveMovement: Horizontal sine wave pattern (variation)
///   - Future: CircleMovement, ZigZagMovement, HomingMovement, etc.
/// 
/// This follows the Strategy Pattern from software design, where the algorithm
/// (movement pattern) is encapsulated and interchangeable at runtime.
/// </summary>
public partial class MovementBehavior : Node
{
    /// <summary>Movement speed in pixels per second - higher = faster movement</summary>
    [Export] public float Speed { get; set; } = 100f;
    
    /// <summary>Base direction vector - some behaviors use this, others override completely</summary>
    [Export] public Vector2 Direction { get; set; } = Vector2.Down;
    
    /// <summary>
    /// Calculate the velocity delta for this frame
    /// 
    /// Parameters:
    ///   - delta: Time since last frame in seconds (for frame-rate independence)
    ///   - currentPosition: Enemy's current world position (for position-based patterns)
    /// 
    /// Returns:
    ///   - Vector2 velocity to add to position this frame
    /// 
    /// This base implementation provides simple directional movement.
    /// Subclasses override this to implement complex patterns.
    /// </summary>
    public virtual Vector2 CalculateMovement(float delta, Vector2 currentPosition)
    {
        return Direction * Speed * delta;
    }
}

/// <summary>
/// Straight Downward Movement - Type 01 "Straight-Shot" enemies
/// 
/// The simplest movement pattern: constant downward motion at specified speed.
/// This is the classic shmup enemy movement seen in 1942 and Galaga, where
/// enemies enter from the top and fly straight down in formation.
/// 
/// Used by:
///   - Type 01 enemies (straight-shot variant)
///   - Basic enemies in early waves
///   - Default fallback when no behavior specified
/// </summary>
public partial class StraightMovement : MovementBehavior
{
    /// <summary>
    /// Returns constant downward velocity
    /// 
    /// Ignores Direction property and currentPosition - always moves down.
    /// Speed determines how fast the enemy descends.
    /// </summary>
    public override Vector2 CalculateMovement(float delta, Vector2 currentPosition)
    {
        return Vector2.Down * Speed * delta;
    }
}

/// <summary>
/// Sine Wave Movement - Horizontal oscillation pattern
/// 
/// Creates a smooth left-right wave pattern while moving downward, similar to
/// enemies in Galaga that weave across the screen. The pattern is defined by:
///   - Amplitude: How far left/right the enemy moves (in pixels)
///   - Frequency: How fast the oscillation occurs (cycles per second)
/// 
/// Mathematical Implementation:
///   X offset = sin(time * frequency) * amplitude
///   Y offset = constant downward speed
/// 
/// This creates engaging, predictable-but-challenging enemy paths that players
/// can learn to dodge. Common in classic shmups for variety and difficulty.
/// </summary>
public partial class SineWaveMovement : MovementBehavior
{
    /// <summary>Maximum horizontal displacement from center path (in pixels)</summary>
    [Export] public float Amplitude { get; set; } = 50f;
    
    /// <summary>Oscillation speed (higher = faster waves, typical range 1-5)</summary>
    [Export] public float Frequency { get; set; } = 2f;
    
    /// <summary>Accumulated time for sine wave calculation (continuous phase)</summary>
    private float _time = 0f;
    
    /// <summary>
    /// Calculate sine wave motion with downward movement
    /// 
    /// The sine function creates smooth oscillation:
    ///   - At time 0: center
    ///   - At π/2: maximum right
    ///   - At π: center
    ///   - At 3π/2: maximum left
    ///   - At 2π: back to center (cycle repeats)
    /// 
    /// Multiplying by delta scales the amplitude appropriately per frame.
    /// The Y component maintains constant downward speed.
    /// </summary>
    public override Vector2 CalculateMovement(float delta, Vector2 currentPosition)
    {
        _time += delta;
        float xOffset = Mathf.Sin(_time * Frequency) * Amplitude * delta;
        return new Vector2(xOffset, Speed * delta);
    }
}

