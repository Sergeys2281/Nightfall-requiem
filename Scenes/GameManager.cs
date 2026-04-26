using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    // --- ПІДКЛЮЧЕННЯ UI ---
    [Export] public ProgressBar ExpBar { get; set; }
    [Export] public Label TimerLabel { get; set; }
    [Export] public PackedScene UpgradeMenuScene { get; set; }

    // --- ДАНІ ГРАВЦЯ ---
    public int CurrentLevel = 1;
    public int CurrentExperience = 0;
    public int ExperienceToNextLevel = 100;

    // --- ЛОГІКА ТАЙМЕРА ТА СКЛАДНОСТІ ---
    private float _timeElapsed = 0.0f;
    private int _difficultyLevel = 0;
    private const float DifficultyStepTime = 30.0f; // Кожні 30 секунд складність росте

    // --- СИСТЕМА ПОЛІПШЕНЬ ---
    private const int MaxActiveSlots = 3;
    private const int MaxPassiveSlots = 3;

    public List<string> ActiveSlots = new List<string> { "Sword" }; // Меч є зі старту
    public List<string> PassiveSlots = new List<string>();
    public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int> { { "Sword", 1 }, {"FireStaff", 1 } };

    public override void _Ready()
    {
        UpdateUI();
    }

    public class UpgradeData
    {
        public string Id;
        public string Title;
        public string Description;
        public bool IsActive; // true = зброя, false = пасивне (статус)
    }

    // Всі існуючі в грі предмети
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

    // Функція генерації 3 варіантів для меню
    public List<UpgradeData> GetUpgradeChoices()
    {
        List<UpgradeData> pool = new List<UpgradeData>();

        foreach (var item in UpgradeDatabase.Values)
        {
            // ПЕРЕВІРКА ЛІМІТУ: Якщо рівень вже 5 або більше, пропускаємо цей предмет
            int currentLvl = GetUpgradeLevel(item.Id);
            if (currentLvl >= 5) continue;
            // Перевіряємо чи це активний слот і чи є для нього місце
            if (item.IsActive)
            {
                if (ActiveSlots.Contains(item.Id)) pool.Add(item); // Вже є, прокачка
                else if (ActiveSlots.Count < MaxActiveSlots) pool.Add(item); // Є вільний слот
            }
            // Перевірка пасивок
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
        // Кожен рівень додає 5% до бази
        return 1.0f + (level * 0.05f);
    }

    public int GetUpgradeLevel(string id)
    {
        return UpgradeLevels.ContainsKey(id) ? UpgradeLevels[id] : 0;
    }

    // Застосування вибору
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
            // Старі пасивки
            if (id == "MaxHP") { player.MaxHealth += 20; player.Heal(20); }
            else if (id == "Speed") { player.Speed += 15.0f; }

            // Нові пасивки
            else if (id == "Armor") { player.Armor += 1; }
            else if (id == "Cooldown") { player.UpdateAttackSpeed(); }
            // Regen працює через таймер гравця

            else if (id == "Aura" || id == "Damage") { player.UpdateAuraStatus(); }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeElapsed += (float)delta;

        UpdateTimerDisplay();

        int newDifficulty = (int)(_timeElapsed / DifficultyStepTime);
        if (newDifficulty > _difficultyLevel)
        {
            _difficultyLevel = newDifficulty;
            OnDifficultyIncreased();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (TimerLabel == null) return;

        // Конвертуємо секунди у хвилини та секунди
        int minutes = (int)_timeElapsed / 60;
        int seconds = (int)_timeElapsed % 60;

        TimerLabel.Text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnDifficultyIncreased()
    {
        GD.Print($"[Складність] Рівень підвищено до: {_difficultyLevel}");

        // Знаходимо спавнер і кажемо йому оновити ліміти
        var spawnerNode = GetTree().Root.FindChild("EnemySpawner", true, false);
        if (spawnerNode is EnemySpawner spawner)
        {
            spawner.UpdateDifficulty(_difficultyLevel);
        }
    }

    // Функція яку вороги викликають при появі щоб дізнатися свій бонус до ХП
    public float GetEnemyHpMultiplier()
    {
        // Додаємо 5% здоров'я за кожен рівень складності
        return 1.0f + (_difficultyLevel * 0.05f);
    }

    // --- СИСТЕМА ДОСВІДУ ---
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
}