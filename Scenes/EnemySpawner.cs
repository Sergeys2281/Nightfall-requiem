using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    // --- НАЛАШТУВАННЯ СПАВНУ ---
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public float SpawnRadius { get; set; } = 600.0f;

    // --- ДИНАМІЧНА СКЛАДНІСТЬ ---
    private int _maxEnemies = 10;
    private Timer _spawnTimer;

    private Node2D _player;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        // Знаходимо гравця на карті
        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = (Node2D)playerNode;
        }

        // Підключаємо таймер спавну
        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
    }

    // Цю функцію викликає GameManager кожні 30 секунд
    public void UpdateDifficulty(int level)
    {
        // 1. Оновлюємо ліміт: 10 + 5 за кожен рівень складності (макс 100)
        _maxEnemies = Math.Min(10 + (level * 5), 100);

        // 2. Оновлюємо швидкість спавну:
        // Починаємо з 1.5 сек. Зменшуємо на 0.08 сек кожен рівень, але не швидше ніж 0.1 сек
        float newWaitTime = 1.5f - (level * 0.08f);
        _spawnTimer.WaitTime = Math.Max(newWaitTime, 0.1f);

        GD.Print($"[Спавнер] Новий ліміт: {_maxEnemies}, Кулдаун спавну: {_spawnTimer.WaitTime:F2} сек");
    }

    private void OnSpawnTimerTimeout()
    {
        // Захист від помилок
        if (_player == null || EnemyScene == null) return;

        // Рахуємо, скільки зараз ворогів на карті (у групі "Enemies")
        int currentEnemies = GetTree().GetNodesInGroup("Enemies").Count;

        // Спавнимо нового монстра ТІЛЬКИ якщо ми ще не досягли поточного ліміту
        if (currentEnemies < _maxEnemies)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        //1 Генеруємо випадковий кут на колі
        float randomAngle = _rng.RandfRange(0, Mathf.Tau);

        // 2 Перетворення кута в координати за межами екрана
        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(randomAngle) * SpawnRadius,
            Mathf.Sin(randomAngle) * SpawnRadius
        );

        // 3 Зміщуємо цю точку відносно поточної позиції гравця
        Vector2 spawnPosition = _player.GlobalPosition + spawnOffset;

        // 4 Створюємо ворога
        var enemy = EnemyScene.Instantiate<CharacterBody2D>();
        enemy.GlobalPosition = spawnPosition;

        // 5 На всякий випадок додаємо ворога в групу прямо при спавні, щоб лічильник ліміту працював ідеально точно
        enemy.AddToGroup("Enemies");

        // 6. Додаємо його у світ
        GetParent().AddChild(enemy);
    }
}