# Test Harness

This project includes a lightweight smoke-test runner to catch regressions in core gameplay behaviors.

## What it tests
- Player shooting spawns bullets
- Enemy takes damage and is freed on death
- Boss spiral spawns enemy bullets
- GameManager spawns enemies and toggles game over/restart path

## How to run (Editor)
1. Open Godot.
2. Open `scenes/test_runner.tscn`.
3. Press Play. The output console will print PASS/FAIL and the app will quit with exit code 0/1.

## How to run (Windows PowerShell, headless)
Replace the path to your Godot executable as needed (Godot 4.x):

```powershell
# From the repo root
& "C:\Program Files\Godot\Godot_v4.2.2-stable_win64.exe" --headless --path . --main-pack res://project.godot --run -- \
    --main-scene res://scenes/test_runner.tscn
```

If your Godot build doesn’t support `--main-scene`, instead temporarily set `run/main_scene` in `project.godot` to `res://scenes/test_runner.tscn`, run headless once, then revert.

## Notes
- The tests are smoke tests, not full physics or time-accurate simulations. They assert essential contracts and will catch scene/script wiring errors quickly.
- On failures, see the printed reason and open `scripts/tests/TestRunner.gd` to adjust or extend.
