using Godot;

public partial class UI : CanvasLayer
{
	private Label _healthLabel = new();
	private Label _scoreLabel = new();
	private Label _scoreTitleLabel = new();
	private ColorRect _healthBar = new();
	private ColorRect _healthBarBg = new();
	private Panel _gameOverPanel = new();
	private Panel _hudPanel = new();
	private int _score;
	private int _health = 3;
	private int _maxHealth = 3;

	public override void _Ready()
	{
		Layer = 100;

		// Create a stylish HUD panel with modern design
		_hudPanel.Position = new Vector2(GetViewport().GetVisibleRect().Size.X - 280, 20);
		_hudPanel.Size = new Vector2(260, 140);
		
		var hudStyle = new StyleBoxFlat();
		hudStyle.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.85f);
		hudStyle.BorderColor = new Color(0.2f, 0.5f, 0.9f, 0.6f);
		hudStyle.SetBorderWidthAll(2);
		hudStyle.SetCornerRadiusAll(12);
		hudStyle.ShadowColor = new Color(0, 0, 0, 0.5f);
		hudStyle.ShadowSize = 8;
		hudStyle.ShadowOffset = new Vector2(0, 4);
		_hudPanel.AddThemeStyleboxOverride("panel", hudStyle);
		AddChild(_hudPanel);

		var hudContainer = new VBoxContainer();
		hudContainer.Position = new Vector2(15, 15);
		hudContainer.AddThemeConstantOverride("separation", 12);
		_hudPanel.AddChild(hudContainer);

		// Score section with title
		_scoreTitleLabel.Text = "SCORE";
		_scoreTitleLabel.HorizontalAlignment = HorizontalAlignment.Right;
		_scoreTitleLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 1f, 0.7f));
		_scoreTitleLabel.AddThemeFontSizeOverride("font_size", 14);
		hudContainer.AddChild(_scoreTitleLabel);

		_scoreLabel.HorizontalAlignment = HorizontalAlignment.Right;
		_scoreLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.5f));
		_scoreLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.8f));
		_scoreLabel.AddThemeFontSizeOverride("font_size", 42);
		_scoreLabel.AddThemeConstantOverride("outline_size", 3);
		hudContainer.AddChild(_scoreLabel);

		// Spacer
		hudContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 8) });

		// Health section
		var healthTitle = new Label { Text = "HEALTH", HorizontalAlignment = HorizontalAlignment.Left };
		healthTitle.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 1f, 0.7f));
		healthTitle.AddThemeFontSizeOverride("font_size", 14);
		hudContainer.AddChild(healthTitle);

		// Health bar container with background
		var healthBarContainer = new Control { CustomMinimumSize = new Vector2(230, 30) };
		hudContainer.AddChild(healthBarContainer);

		_healthBarBg.Size = new Vector2(230, 30);
		_healthBarBg.Position = new Vector2(0, 0);
		var barBgStyle = new StyleBoxFlat();
		barBgStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
		barBgStyle.SetCornerRadiusAll(6);
		barBgStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
		barBgStyle.SetBorderWidthAll(1);
		_healthBarBg.Color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
		healthBarContainer.AddChild(_healthBarBg);

		// Gradient health bar
		_healthBar.Size = new Vector2(230, 30);
		_healthBar.Position = new Vector2(0, 0);
		
		// Create gradient for health bar
		var gradient = new Gradient();
		gradient.SetColor(0, new Color(0.2f, 1f, 0.5f)); // Green
		gradient.SetColor(1, new Color(0.1f, 0.8f, 0.4f)); // Darker green
		
		healthBarContainer.AddChild(_healthBar);

		// Health text overlay with shadow
		_healthLabel.Position = new Vector2(10, 5);
		_healthLabel.AddThemeColorOverride("font_color", Colors.White);
		_healthLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
		_healthLabel.AddThemeFontSizeOverride("font_size", 18);
		_healthLabel.AddThemeConstantOverride("outline_size", 2);
		healthBarContainer.AddChild(_healthLabel);

		// Stunning Game Over screen
		_gameOverPanel.Visible = false;
		_gameOverPanel.AnchorLeft = 0.25f;
		_gameOverPanel.AnchorRight = 0.75f;
		_gameOverPanel.AnchorTop = 0.25f;
		_gameOverPanel.AnchorBottom = 0.75f;
		
		var panelStyle = new StyleBoxFlat();
		panelStyle.BgColor = new Color(0.08f, 0.08f, 0.15f, 0.97f);
		panelStyle.BorderColor = new Color(0.4f, 0.7f, 1f);
		panelStyle.SetBorderWidthAll(4);
		panelStyle.SetCornerRadiusAll(20);
		panelStyle.ShadowColor = new Color(0, 0, 0, 0.8f);
		panelStyle.ShadowSize = 20;
		panelStyle.ShadowOffset = new Vector2(0, 8);
		_gameOverPanel.AddThemeStyleboxOverride("panel", panelStyle);
		AddChild(_gameOverPanel);

		var vbox = new VBoxContainer();
		vbox.AnchorRight = 1;
		vbox.AnchorBottom = 1;
		vbox.Alignment = BoxContainer.AlignmentMode.Center;
		vbox.AddThemeConstantOverride("separation", 20);
		_gameOverPanel.AddChild(vbox);

		// Game Over title with glow
		var title = new Label { Text = "GAME OVER", HorizontalAlignment = HorizontalAlignment.Center };
		title.AddThemeColorOverride("font_color", new Color(1f, 0.2f, 0.3f));
		title.AddThemeColorOverride("font_outline_color", new Color(0.5f, 0, 0.1f));
		title.AddThemeFontSizeOverride("font_size", 64);
		title.AddThemeConstantOverride("outline_size", 4);
		vbox.AddChild(title);

		// Decorative line
		var line = new ColorRect();
		line.CustomMinimumSize = new Vector2(400, 3);
		line.Color = new Color(0.4f, 0.7f, 1f, 0.6f);
		line.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		vbox.AddChild(line);

		// Final score with enhanced styling
		var scoreContainer = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		scoreContainer.AddThemeConstantOverride("separation", 5);
		vbox.AddChild(scoreContainer);

		var scoreTitle = new Label { Text = "FINAL SCORE", HorizontalAlignment = HorizontalAlignment.Center };
		scoreTitle.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 1f, 0.8f));
		scoreTitle.AddThemeFontSizeOverride("font_size", 20);
		scoreContainer.AddChild(scoreTitle);

		var finalScore = new Label { Text = "0", HorizontalAlignment = HorizontalAlignment.Center, Name = "FinalScore" };
		finalScore.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.5f));
		finalScore.AddThemeColorOverride("font_outline_color", new Color(0, 0.5f, 0.2f));
		finalScore.AddThemeFontSizeOverride("font_size", 56);
		finalScore.AddThemeConstantOverride("outline_size", 4);
		scoreContainer.AddChild(finalScore);

		vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 30) });

		// Modern button container
		var btnContainer = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		btnContainer.AddThemeConstantOverride("separation", 25);
		vbox.AddChild(btnContainer);

		// Retry button with modern styling
		var retryBtn = new Button { Text = "  ?  RETRY  " };
		var retryStyle = new StyleBoxFlat();
		retryStyle.BgColor = new Color(0.2f, 0.6f, 1f);
		retryStyle.SetCornerRadiusAll(8);
		retryStyle.ContentMarginTop = 12;
		retryStyle.ContentMarginBottom = 12;
		retryStyle.ContentMarginLeft = 25;
		retryStyle.ContentMarginRight = 25;
		retryBtn.AddThemeStyleboxOverride("normal", retryStyle);
		
		var retryHoverStyle = new StyleBoxFlat();
		retryHoverStyle.BgColor = new Color(0.3f, 0.7f, 1f);
		retryHoverStyle.SetCornerRadiusAll(8);
		retryHoverStyle.ContentMarginTop = 12;
		retryHoverStyle.ContentMarginBottom = 12;
		retryHoverStyle.ContentMarginLeft = 25;
		retryHoverStyle.ContentMarginRight = 25;
		retryBtn.AddThemeStyleboxOverride("hover", retryHoverStyle);
		
		retryBtn.AddThemeColorOverride("font_color", Colors.White);
		retryBtn.AddThemeFontSizeOverride("font_size", 24);
		retryBtn.Pressed += () => { GetTree().Paused = false; GetTree().ReloadCurrentScene(); };
		btnContainer.AddChild(retryBtn);

		// Quit button with subtle styling
		var quitBtn = new Button { Text = "  ?  QUIT  " };
		var quitStyle = new StyleBoxFlat();
		quitStyle.BgColor = new Color(0.3f, 0.3f, 0.35f);
		quitStyle.SetCornerRadiusAll(8);
		quitStyle.ContentMarginTop = 12;
		quitStyle.ContentMarginBottom = 12;
		quitStyle.ContentMarginLeft = 25;
		quitStyle.ContentMarginRight = 25;
		quitBtn.AddThemeStyleboxOverride("normal", quitStyle);
		
		var quitHoverStyle = new StyleBoxFlat();
		quitHoverStyle.BgColor = new Color(0.8f, 0.3f, 0.3f);
		quitHoverStyle.SetCornerRadiusAll(8);
		quitHoverStyle.ContentMarginTop = 12;
		quitHoverStyle.ContentMarginBottom = 12;
		quitHoverStyle.ContentMarginLeft = 25;
		quitHoverStyle.ContentMarginRight = 25;
		quitBtn.AddThemeStyleboxOverride("hover", quitHoverStyle);
		
		quitBtn.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
		quitBtn.AddThemeFontSizeOverride("font_size", 24);
		quitBtn.Pressed += () => GetTree().Quit();
		btnContainer.AddChild(quitBtn);

		UpdateHUD();
	}

	public void SetHealth(int health)
	{
		_health = health;
		UpdateHUD();
		if (_health <= 0) ShowGameOver();
	}

	public void AddScore(int points)
	{
		_score += points;
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		_healthLabel.Text = $"? {_health} / {_maxHealth}";
		_scoreLabel.Text = $"{_score:N0}";
		
		// Animate health bar width with smooth transition
		float healthPercent = (float)_health / _maxHealth;
		_healthBar.Size = new Vector2(230 * healthPercent, 30);
		
		// Dynamic color based on health level with smooth gradients
		if (healthPercent > 0.6f)
		{
			_healthBar.Color = new Color(0.2f, 1f, 0.5f); // Bright green
			_scoreTitleLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.7f, 1f, 0.7f));
		}
		else if (healthPercent > 0.3f)
		{
			_healthBar.Color = new Color(1f, 0.8f, 0f); // Warning yellow
			_scoreTitleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.7f, 0.3f, 0.7f));
		}
		else
		{
			_healthBar.Color = new Color(1f, 0.2f, 0.3f); // Critical red
			_scoreTitleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f, 0.7f));
		}
	}

	private void ShowGameOver()
	{
		_gameOverPanel.Visible = true;
		var finalScore = _gameOverPanel.GetNode<Label>("VBoxContainer/VBoxContainer/FinalScore");
		finalScore.Text = $"{_score:N0}";
		GetTree().Paused = true;
	}
}
