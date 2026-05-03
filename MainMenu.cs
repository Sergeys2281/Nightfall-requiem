using Godot;
using System;

public partial class MainMenu : Control
{
    private Control _mainPanel;
    private Control _levelSelectPanel;
    private Label _highScoreLabel;

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


    private void OnPlayButtonPressed()
    {
        _mainPanel.Hide();
        _levelSelectPanel.Show();
    }

    private void OnBackButtonPressed()
    {
        _levelSelectPanel.Hide();
        _mainPanel.Show();
    }


    private void LoadLevel(string levelPath)
    {
        GD.Print($"Вибрано рівень: {levelPath}");

        GameManager.SelectedLevelPath = levelPath;

        GetTree().ChangeSceneToFile("res://main.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}