using Godot;
using System;

public partial class GameManager : Node
{
    public int CurrentLevel = 1;
    public int CurrentExperience = 0;
    public int ExperienceToNextLevel = 100;

    [Export] public ProgressBar ExpBar { get; set; }

    public override void _Ready()
    {
        UpdateUI();
    }

    public void AddExperience(int amount)
    {
        CurrentExperience += amount;
        GD.Print($"Поточний досвід: {CurrentExperience} / {ExperienceToNextLevel}");

        if (CurrentExperience >= ExperienceToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        CurrentLevel++;
        CurrentExperience -= ExperienceToNextLevel;
        ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.2f);
        GD.Print($"ПІДВИЩЕННЯ РІВНЯ! Ти досяг рівня {CurrentLevel}");
    }

    private void UpdateUI()
    {
        if (ExpBar != null)
        {
            ExpBar.MaxValue = ExperienceToNextLevel;
            ExpBar.Value = CurrentExperience;
        }
        else
        {
            GD.Print("ПОМИЛКА: ExpBar не підключено в Інспекторі!");
        }
    }
}