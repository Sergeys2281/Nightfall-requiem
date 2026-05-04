using Godot;
using System;

/// <summary>
/// Клас, що відповідає за екран поразки (Game Over).
/// Відображає фінальний рахунок, час виживання гравця та обробляє збереження нового рекорду.
/// </summary>
public partial class GameOverMenu : Control
{
    [Export] public Label ScoreLabel;
    [Export] public Label TimeLabel;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Приховує екран поразки при старті гри, щоб він не заважав ігровому процесу.
    /// </summary>
    public override void _Ready()
    {
        Hide();
    }

    /// <summary>
    /// Активує екран поразки, зупиняє гру (ставить на паузу), оновлює статистику
    /// та перевіряє, чи було встановлено новий рекорд.
    /// </summary>
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

    /// <summary>
    /// Обробник події натискання кнопки повернення до головного меню.
    /// Знімає гру з паузи та завантажує сцену стартового меню.
    /// </summary>
    public void _on_menu_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}