using Godot;

public partial class HUD : CanvasLayer
{
	private Label? _scoreLabel;
	private Label? _highScoreLabel;
	private Label? _healthLabel;
	private Label? _gameOverLabel;
	
	public override void _Ready()
	{
		// Get UI elements
		_scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
		_highScoreLabel = GetNodeOrNull<Label>("HighScoreLabel");
		_healthLabel = GetNodeOrNull<Label>("HealthLabel");
		_gameOverLabel = GetNodeOrNull<Label>("GameOverLabel");
		
		// Initialize display
		UpdateScore(0, 0);
		UpdateHealth(5);
		HideGameOver();
	}
	
	public void UpdateScore(int currentScore, int highScore)
	{
		if (_scoreLabel != null)
		{
			_scoreLabel.Text = $"Score: {currentScore:N0}";
		}
		
		if (_highScoreLabel != null)
		{
			_highScoreLabel.Text = $"High Score: {highScore:N0}";
		}
	}
	
	public void UpdateHealth(int health)
	{
		if (_healthLabel != null)
		{
			_healthLabel.Text = $"Health: {health}";
		}
	}
	
	public void ShowGameOver(int finalScore, int highScore)
	{
		if (_gameOverLabel != null)
		{
			_gameOverLabel.Text = $"GAME OVER\nFinal Score: {finalScore:N0}\nHigh Score: {highScore:N0}\nRestarting in 2 seconds...";
			_gameOverLabel.Visible = true;
		}
	}
	
	public void HideGameOver()
	{
		if (_gameOverLabel != null)
		{
			_gameOverLabel.Visible = false;
		}
	}
}
