extends Node

# Simple, headless-friendly smoke tests for core gameplay contracts.
# Prints results and quits the game with code 0/1.

var _passes := 0
var _fails := 0

func _ready() -> void:
	await get_tree().process_frame
	await _run()
	_print_summary_and_quit()

func _log_pass(test_name: String) -> void:
	_passes += 1
	print("[PASS] ", test_name)

func _log_fail(test_name: String, why: String = "") -> void:
	_fails += 1
	printerr("[FAIL] %s %s" % [test_name, ("- " + why) if why != "" else ""]) 

func _print_summary_and_quit() -> void:
	var total := _passes + _fails
	print("==== TEST SUMMARY ====\nTotal:", total, " Pass:", _passes, " Fail:", _fails)
	get_tree().quit(_fails)

func _run() -> void:
	await _test_player_shoots()
	await _test_enemy_takes_damage_and_dies()
	await _test_boss_spiral_spawns_bullets()
	await _test_boss_z_spiral_spawns_bullets()
	await _test_game_manager_spawns_and_restart()
	await _test_game_manager_spawns_boss2_fallback()

# --- Helpers ---
func _add_to_scene(n: Node) -> void:
	get_tree().root.add_child(n)

func _remove_from_scene(n: Node) -> void:
	if is_instance_valid(n):
		n.queue_free()
		await get_tree().process_frame

func _yield_frames(frames: int) -> void:
	for i in frames:
		await get_tree().process_frame

# --- Tests ---
func _test_player_shoots() -> void:
	var scene := load("res://scenes/main.tscn")
	var inst: Node = scene.instantiate() as Node
	_add_to_scene(inst)
	await _yield_frames(2)
	var player: Variant = inst.get_node("Player")
	if not player:
		_log_fail("player exists")
		_remove_from_scene(inst)
		return
	# Simulate hold fire
	player._fire_timer = 0.0
	player.BulletScene = load("res://scenes/player_bullet.tscn")
	player._shoot()
	await _yield_frames(1)
	var bullets := get_tree().get_nodes_in_group("player_bullets")
	if bullets.size() >= 1:
		_log_pass("player shoots spawns bullet")
	else:
		_log_fail("player shoots spawns bullet")
	_remove_from_scene(inst)

func _test_enemy_takes_damage_and_dies() -> void:
	var enemy_scene := load("res://scenes/enemy.tscn")
	var enemy: Variant = enemy_scene.instantiate() as Node
	_add_to_scene(enemy)
	await _yield_frames(1)
	if not enemy.has_method("TakeDamage"):
		_log_fail("enemy has TakeDamage")
		_remove_from_scene(enemy)
		return
	var start_health: int = int(enemy.GetHealth())
	enemy.TakeDamage(start_health)
	await _yield_frames(1)
	if not is_instance_valid(enemy):
		_log_pass("enemy dies at 0 health")
	else:
		_log_fail("enemy dies at 0 health")
		_remove_from_scene(enemy)

func _test_boss_spiral_spawns_bullets() -> void:
	var boss_scene := load("res://scenes/boss.tscn")
	var boss: Variant = boss_scene.instantiate() as Node
	_add_to_scene(boss)
	await _yield_frames(2)
	if boss.has_method("_shoot_spiral"):
		boss._shoot_spiral()
		await _yield_frames(1)
		var bullets := get_tree().get_nodes_in_group("enemy_bullets")
		if bullets.size() >= 1:
			_log_pass("boss spiral spawns bullets")
		else:
			_log_fail("boss spiral spawns bullets")
	else:
		_log_fail("boss has shoot spiral")
	_remove_from_scene(boss)

func _test_boss_z_spiral_spawns_bullets() -> void:
	var boss_scene := load("res://scenes/boss_z.tscn")
	var boss: Variant = boss_scene.instantiate() as Node
	_add_to_scene(boss)
	await _yield_frames(2)
	if boss.has_method("_shoot_spiral"):
		boss._shoot_spiral()
		await _yield_frames(1)
		var bullets := get_tree().get_nodes_in_group("enemy_bullets")
		if bullets.size() >= 1:
			_log_pass("boss_z spiral spawns bullets")
		else:
			_log_fail("boss_z spiral spawns bullets")
	else:
		_log_fail("boss_z has shoot spiral")
	_remove_from_scene(boss)

func _test_game_manager_spawns_and_restart() -> void:
	var gm := preload("res://scripts/GameManager.gd").new()
	_add_to_scene(gm)
	await _yield_frames(2)
	# Force spawn
	gm._spawn_enemy()
	await _yield_frames(1)
	var enemies := get_tree().get_nodes_in_group("enemies")
	if enemies.size() >= 1:
		_log_pass("gm spawn enemy")
	else:
		_log_fail("gm spawn enemy")
	# Simulate player hits to zero to trigger restart path
	for i in gm.PlayerHealth:
		gm.OnPlayerHit()
	await _yield_frames(1)
	if gm._game_over:
		_log_pass("gm game over set")
	else:
		_log_fail("gm game over set")
	# Cleanup
	_remove_from_scene(gm)

func _test_game_manager_spawns_boss2_fallback() -> void:
	var gm := preload("res://scripts/GameManager.gd").new()
	_add_to_scene(gm)
	# Ensure no explicit BossScene2 is set so fallback loads
	gm.BossScene2 = null
	# Force immediate spawn
	gm._boss2_spawned = false
	gm._boss2_spawn_timer = 0.0
	# Trigger process once; should attempt spawn
	gm._process(0.016)
	await _yield_frames(1)
	var bosses := get_tree().get_nodes_in_group("boss")
	if bosses.size() >= 1:
		_log_pass("gm spawns boss2 via fallback")
	else:
		_log_fail("gm spawns boss2 via fallback", "no node in 'boss' group found")
	_remove_from_scene(gm)
