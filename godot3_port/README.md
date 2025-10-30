Godot 3 Port Notes

This folder contains C# scripts converted for Godot 3.5 Mono.

What this includes:
- Converted C# scripts (Godot 3 API: no partial classes, float delta, Sprite/AnimatedSprite, signals via Connect).

What you still need to do in Godot 3 Editor:
- Create a new Godot 3.5 Mono project.
- Recreate scenes replacing Godot 4 nodes with Godot 3 counterparts:
  - AnimatedSprite2D -> AnimatedSprite
  - Sprite2D -> Sprite
  - Area2D, Node2D, CanvasLayer remain the same.
- Attach the converted scripts from `godot3_port/scripts/` to the appropriate nodes.
- Recreate Input Map: ui_left, ui_right, ui_up, ui_down, ui_accept.

Notes:
- Signals are connected in _Ready using Connect() in C# for Godot 3.
- Properties exported in Godot 4 were changed to exported fields in Godot 3 for reliability.
- Some APIs differ slightly; verify UI Control anchoring and parallax properties after import.


