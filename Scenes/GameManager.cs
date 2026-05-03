using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    [Export] public ProgressBar ExpBar { get; set; }
    [Export] public Label TimerLabel { get; set; }
    [Export] public PackedScene UpgradeMenuScene { get; set; }

    public int CurrentLevel = 1;
    public int CurrentExperience = 0;
    public int ExperienceToNextLevel = 100;

    private float _timeElapsed = 0.0f;
    private int _difficultyLevel = 0;
    private const float DifficultyStepTime = 30.0f;

    private const int MaxActiveSlots = 3;
    private const int MaxPassiveSlots = 3;

    public List<string> ActiveSlots = new List<string> { "Sword" };
    public List<string> PassiveSlots = new List<string>();
    public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int> {  };


    public static string SelectedLevelPath = "res://scenes/level_desert.tscn";

    public static int CurrentScore = 0;
    public static double RunTime = 0.0;
    public static int HighScore = 0;

    private const string SavePath = "user://highscore.save";

    private bool _isHordeMode = false;

    public override void _Ready()
    {
        CurrentScore = 0;
        RunTime = 0.0;

        var levelContainer = GetParent().GetNode<Node>("LevelContainer");

        foreach (Node child in levelContainer.GetChildren())
        {
            if (child.Name != "Player2")
            {
                levelContainer.RemoveChild(child);
                child.QueueFree();
            }
        }

        var levelScene = GD.Load<PackedScene>(SelectedLevelPath);
        if (levelScene != null)
        {
            var newLevel = levelScene.Instantiate();

            levelContainer.AddChild(newLevel);

            levelContainer.MoveChild(newLevel, 0);

            GD.Print($"Рівень {SelectedLevelPath} успішно завантажено в Main!");
        }
        else
        {
            GD.PrintErr("Не вдалося завантажити сцену рівня! Перевір шлях.");
        }

        UpdateUI();

        CallDeferred("ApplyUpgrade", "Sword");
    }
    public override void _Process(double delta)
    {
        RunTime += delta;
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsed += (float)delta;

        UpdateTimerDisplay();

        if (_timeElapsed >= 1800.0f && !_isHordeMode)
        {
            _isHordeMode = true;
            TriggerHordeMode();
        }

        int newDifficulty = (int)(_timeElapsed / DifficultyStepTime);
        if (newDifficulty > _difficultyLevel)
        {
            _difficultyLevel = newDifficulty;

            if (!_isHordeMode)
            {
                OnDifficultyIncreased();
            }
        }
    }

    private void TriggerHordeMode()
    {
        GD.Print("💀 30 ХВИЛИН! ПОЧИНАЄТЬСЯ НЕСКІНЧЕННА ОРДА!");

        var spawnerNode = GetTree().Root.FindChild("EnemySpawner", true, false);
        if (spawnerNode is EnemySpawner spawner)
        {
            spawner.ActivateHordeMode();
        }
    }

    public class UpgradeData
    {
        public string Id;
        public string Title;
        public string Description;
        public bool IsActive;
    }

    public Dictionary<string, UpgradeData> UpgradeDatabase = new Dictionary<string, UpgradeData>
    {
        { "Sword", new UpgradeData { Id = "Sword", Title = "🗡️ Поліпшення Меча", Description = "Збільшує шкоду від меча.", IsActive = true } },
        { "Bow", new UpgradeData { Id = "Bow", Title = "🏹 Лук", Description = "Стріляє віялом стріл у випадковому напрямку.", IsActive = true } },
        { "Aura", new UpgradeData { Id = "Aura", Title = "✨ Аура", Description = "Наносить періодичну шкоду всім ворогам навколо.", IsActive = true } },
        { "Staff", new UpgradeData {Id = "Staff", Title = "🌊 Посох Води", Description = "Рівні 1-4: +1 снаряд. Рівень 5: Величезний бонус до розміру.", IsActive = true } },
        { "FireStaff", new UpgradeData { Id = "FireStaff", Title = "🔥 Вогняний Посох", Description = "Залишає вогняні калюжі. Рівень збільшує час та радіус.", IsActive = true } },
        { "MaxHP", new UpgradeData { Id = "MaxHP", Title = "❤️ Макс. ХП", Description = "Збільшує макс. здоров'я на 20 і лікує.", IsActive = false } },
        { "Speed", new UpgradeData { Id = "Speed", Title = "👟 Швидкість", Description = "Пришвидшує біг на 15 одиниць.", IsActive = false } },
        { "Damage", new UpgradeData { Id = "Damage", Title = "💪 Сила", Description = "Збільшує загальну шкоду на 5% за рівень.", IsActive = false } },
        { "Armor", new UpgradeData { Id = "Armor", Title = "🛡️ Тіньова Броня", Description = "Зменшує отримувану шкоду на 1.", IsActive = false } },
        { "Regen", new UpgradeData { Id = "Regen", Title = "🩸 Регенерація", Description = "Відновлює 1 ХП кожні 5 секунд.", IsActive = false } },
        { "Cooldown", new UpgradeData { Id = "Cooldown", Title = "⏱️ Жага Крові", Description = "Зброя атакує на 10% швидше.", IsActive = false } }
    };

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public List<UpgradeData> GetUpgradeChoices()
    {
        List<UpgradeData> pool = new List<UpgradeData>();

        foreach (var item in UpgradeDatabase.Values)
        {
            int currentLvl = GetUpgradeLevel(item.Id);
            if (currentLvl >= 5) continue;
            if (item.IsActive)
            {
                if (ActiveSlots.Contains(item.Id)) pool.Add(item);
                else if (ActiveSlots.Count < MaxActiveSlots) pool.Add(item);
            }
            else
            {
                if (PassiveSlots.Contains(item.Id)) pool.Add(item);
                else if (PassiveSlots.Count < MaxPassiveSlots) pool.Add(item);
            }
        }

        return pool.OrderBy(x => _rng.Randf()).Take(3).ToList();
    }

    public float GetStrengthMultiplier()
    {
        int level = UpgradeLevels.ContainsKey("Damage") ? UpgradeLevels["Damage"] : 0;
        return 1.0f + (level * 0.05f);
    }

    public int GetUpgradeLevel(string id)
    {
        return UpgradeLevels.ContainsKey(id) ? UpgradeLevels[id] : 0;
    }

    public void ApplyUpgrade(string id)
    {
        var data = UpgradeDatabase[id];

        if (data.IsActive && !ActiveSlots.Contains(id)) ActiveSlots.Add(id);
        else if (!data.IsActive && !PassiveSlots.Contains(id)) PassiveSlots.Add(id);

        if (UpgradeLevels.ContainsKey(id)) UpgradeLevels[id]++;
        else UpgradeLevels[id] = 1;

        GD.Print($"[Поліпшення] Взято: {data.Title} (Рівень {UpgradeLevels[id]})");

        var player = GetTree().GetFirstNodeInGroup("Player") as Player;
        if (player != null)
        {
            if (id == "MaxHP") { player.MaxHealth += 20; player.Heal(20); }
            else if (id == "Speed") { player.Speed += 15.0f; }

            else if (id == "Armor") { player.Armor += 1; }
            else if (id == "Cooldown") { player.UpdateAttackSpeed(); }

            else if (id == "Aura" || id == "Damage") { player.UpdateAuraStatus(); }
        }

        var upgradesUI = GetTree().CurrentScene.GetNodeOrNull<UpgradesDisplay>("UI/UpgradesDisplay");

        if (upgradesUI != null)
        {
            upgradesUI.AddOrUpdateUpgrade(data.Title, UpgradeLevels[id]);
        }
    }

    private void UpdateTimerDisplay()
    {
        if (TimerLabel == null) return;

        int minutes = (int)_timeElapsed / 60;
        int seconds = (int)_timeElapsed % 60;

        TimerLabel.Text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnDifficultyIncreased()
    {
        GD.Print($"[Складність] Рівень підвищено до: {_difficultyLevel}");

        var spawnerNode = GetTree().Root.FindChild("EnemySpawner", true, false);
        if (spawnerNode is EnemySpawner spawner)
        {
            spawner.UpdateDifficulty(_difficultyLevel);
        }
    }

    public float GetEnemyHpMultiplier()
    {
        return 1.0f + (_difficultyLevel * 0.05f);
    }

    public void AddExperience(int amount)
    {
        CurrentExperience += amount;
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
        GD.Print($"[LEVEL UP] Тепер у тебе {CurrentLevel} рівень!");

        if (UpgradeMenuScene != null)
        {
            GetTree().Paused = true;

            var menu = UpgradeMenuScene.Instantiate<Control>();

            var uiLayer = GetNodeOrNull<CanvasLayer>("../UI");
            if (uiLayer != null)
            {
                uiLayer.AddChild(menu);
            }
            else
            {
                GD.PrintErr("Не знайдено вузол UI для розміщення меню!");
            }
        }
    }

    private void UpdateUI()
    {
        if (ExpBar != null)
        {
            ExpBar.MaxValue = ExperienceToNextLevel;
            ExpBar.Value = CurrentExperience;
        }
    }

    public static void SaveHighScore()
    {
        // Відкриваємо файл для запису
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.Store32((uint)HighScore);
        }
    }

    public static void LoadHighScore()
    {
        if (FileAccess.FileExists(SavePath))
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                HighScore = (int)file.Get32();
            }
        }
    }
}