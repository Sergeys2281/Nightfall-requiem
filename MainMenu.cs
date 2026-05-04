using Godot;
using System;

/// <summary>
/// Клас, що керує логікою головного меню гри.
/// Відповідає за навігацію між панелями (головний екран та вибір рівня), 
/// відображення найкращого рахунку та запуск обраного ігрового рівня.
/// </summary>
public partial class MainMenu : Control
{
    private Control _mainPanel;
    private Control _levelSelectPanel;
    private Label _highScoreLabel;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Завантажує збережений рекорд, ініціалізує посилання на UI-елементи,
    /// підключає обробники подій до кнопок та встановлює початковий стан меню.
    /// </summary>
    public override void _Ready()
    {
        GameManager.LoadHighScore();

        _highScoreLabel = GetNode<Label>("LevelSelectPanel/VBoxContainer/HighScoreLabel");

        if (_highScoreLabel != null)
        {
            _highScoreLabel.Text = $"Найкращий рахунок: {GameManager.HighScore}";
        }

        _mainPanel = GetNode<Control>("MainPanel");
        _levelSelectPanel = GetNode<Control>("LevelSelectPanel");

        var playButton = GetNode<Button>("MainPanel/VBoxContainer/PlayButton");
        var quitButton = GetNode<Button>("MainPanel/VBoxContainer/QuitButton");

        playButton.Pressed += OnPlayButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;

        var forestButton = GetNode<Button>("LevelSelectPanel/VBoxContainer/ForestButton");
        var desertButton = GetNode<Button>("LevelSelectPanel/VBoxContainer/DesertButton");
        var backButton = GetNode<Button>("LevelSelectPanel/VBoxContainer/BackButton");

        forestButton.Pressed += () => LoadLevel("res://scenes/level_forest.tscn");
        desertButton.Pressed += () => LoadLevel("res://scenes/level_desert.tscn");
        backButton.Pressed += OnBackButtonPressed;

        _mainPanel.Show();
        _levelSelectPanel.Hide();
    }

    /// <summary>
    /// Обробник події натискання кнопки "Грати" (Play).
    /// Приховує головну панель меню та відкриває панель вибору рівня.
    /// </summary>
    private void OnPlayButtonPressed()
    {
        _mainPanel.Hide();
        _levelSelectPanel.Show();
    }

    /// <summary>
    /// Обробник події натискання кнопки "Назад" (Back) у вікні вибору рівня.
    /// Повертає гравця до початкового екрана головного меню.
    /// </summary>
    private void OnBackButtonPressed()
    {
        _levelSelectPanel.Hide();
        _mainPanel.Show();
    }

    /// <summary>
    /// Зберігає шлях до обраного рівня у глобальний менеджер та завантажує основну ігрову сцену.
    /// </summary>
    /// <param name="levelPath">Внутрішній шлях рушія (res://) до файлу сцени обраного рівня.</param>
    private void LoadLevel(string levelPath)
    {
        GD.Print($"Вибрано рівень: {levelPath}");

        GameManager.SelectedLevelPath = levelPath;

        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    /// <summary>
    /// Обробник події натискання кнопки "Вихід" (Quit).
    /// Повністю завершує роботу програми та закриває гру.
    /// </summary>
    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}