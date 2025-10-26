# Enemy System for WipShmup3

## What I've Created

I've built a complete enemy system for your shmup game! Here's what's included:

### 🎯 **Enemy Features**
- **Enemy.cs**: Smart enemy AI that moves down the screen
- **Shooting**: Enemies automatically fire bullets at the player
- **Health System**: Enemies take 3 hits to destroy
- **Visual Effects**: Flash red when hit, orange explosion when destroyed
- **Scoring**: Each enemy destroyed gives 100 points

### 🔫 **Player Combat**
- **PlayerBullet.cs**: Player can now shoot back!
- **Auto-fire**: Player shoots automatically every 0.3 seconds
- **Collision Detection**: Bullets hit enemies and deal damage

### 🎮 **Game Management**
- **GameManager.cs**: Handles spawning, scoring, and game over
- **UI**: Score and health display
- **Enemy Spawning**: New enemies appear every 2 seconds
- **Game Over**: When player health reaches 0

### 🎨 **Visual Design**
- **Enemy**: Red square that moves down and shoots
- **Enemy Bullets**: Red bullets that move down
- **Player Bullets**: Blue bullets that move up
- **Health/Score Display**: Top-left corner UI

## How to Play

1. **Movement**: Use WASD or arrow keys to move your ship
2. **Shooting**: Player shoots automatically - just focus on dodging!
3. **Objective**: Destroy enemies and avoid their bullets
4. **Survival**: Don't let your health reach 0!

## Files Created/Modified

### New Files:
- `Enemy.cs` - Enemy behavior and AI
- `EnemyBullet.cs` - Enemy projectile system
- `PlayerBullet.cs` - Player projectile system
- `GameManager.cs` - Game state management
- `enemy.tscn` - Enemy scene
- `enemy_bullet.tscn` - Enemy bullet scene
- `player_bullet.tscn` - Player bullet scene

### Modified Files:
- `Player.cs` - Added shooting and damage system
- `main.tscn` - Integrated all systems together

## Running the Game

1. Open the project in Godot 4.5
2. Make sure C# support is enabled
3. Run the main scene
4. Use WASD to move and avoid enemy bullets!

## Game Features

- ✅ Enemies spawn from the top
- ✅ Enemies shoot at the player
- ✅ Player can shoot back
- ✅ Collision detection works
- ✅ Health system for both player and enemies
- ✅ Scoring system
- ✅ Game over screen
- ✅ Visual feedback for hits

The enemy system is now fully functional and ready to play! 🚀