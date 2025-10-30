extends CanvasLayer

var _score_label: Label
var _high_score_label: Label
var _health_label: Label
var _game_over_label: Label

func _ready() -> void:
    _score_label = get_node_or_null("ScoreLabel")
    _high_score_label = get_node_or_null("HighScoreLabel")
    _health_label = get_node_or_null("HealthLabel")
    _game_over_label = get_node_or_null("GameOverLabel")
    UpdateScore(0, 0)
    UpdateHealth(5)
    HideGameOver()

func UpdateScore(current_score: int, high_score: int) -> void:
    if _score_label:
        _score_label.text = "Score: %s" % [String.num_int64(current_score)]
    if _high_score_label:
        _high_score_label.text = "High Score: %s" % [String.num_int64(high_score)]

func UpdateHealth(health: int) -> void:
    if _health_label:
        _health_label.text = "Health: %d" % health

func ShowGameOver(final_score: int, high_score: int) -> void:
    if _game_over_label:
        _game_over_label.text = "GAME OVER\nFinal Score: %s\nHigh Score: %s\nRestarting in 2 seconds..." % [String.num_int64(final_score), String.num_int64(high_score)]
        _game_over_label.visible = true

func HideGameOver() -> void:
    if _game_over_label:
        _game_over_label.visible = false

