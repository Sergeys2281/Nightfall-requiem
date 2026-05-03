using Godot;
using System;

public partial class GameOverMenu : Control
{
    [Export] public Label ScoreLabel;
    [Export] public Label TimeLabel;

    public override void _Ready()
    {
        Hide();
    }

    public void ShowGameOver()
    {
        if (GameManager.CurrentScore > GameManager.HighScore)
        {
            GameManager.HighScore = GameManager.CurrentScore;
            GameManager.SaveHighScore();
            GD.Print($"Новий рекорд збережено: {GameManager.HighScore}");
        }

        int minutes = (int)(GameManager.RunTime / 60);
        int seconds = (int)(GameManager.RunTime % 60);

        if (ScoreLabel != null)
            ScoreLabel.Text = $"Рахунок: {GameManager.CurrentScore}";

        if (TimeLabel != null)
            TimeLabel.Text = $"Час виживання: {minutes:00}:{seconds:00}";

        var GameOverSound = GetNodeOrNull<AudioStreamPlayer>("GameOverSound");
        if (GameOverSound != null) GameOverSound.Play();

        GetTree().Paused = true;
        Show();
    }

    public void _on_menu_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}