using Godot;
using System;

/// <summary>
/// Клас, що відповідає за генерацію (спавн) ворогів у грі.
/// Керує складністю, лімітами кількості ворогів на екрані, відстеженням часу для появи босів,
/// а також гарантує, що вороги з'являтимуться лише в межах ігрової карти.
/// </summary>
public partial class EnemySpawner : Node2D
{
    /// <summary>
    /// Глобальний множник здоров'я для всіх нових ворогів. 
    /// Може використовуватися для загального ускладнення гри.
    /// </summary>
    [Export] public float HealthMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// Шаблон сцени базового ворога, який буде створюватися спавнером.
    /// </summary>
    [Export] public PackedScene EnemyScene { get; set; }

    /// <summary>
    /// Радіус (у пікселях) навколо гравця, на якому з'являтимуться вороги.
    /// </summary>
    [Export] public float SpawnRadius { get; set; } = 600.0f;

    private int _maxEnemies = 10;
    private Timer _spawnTimer;

    private Node2D _player;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    // Межі карти для обмеження зони спавну
    private float _minX = -10000, _maxX = 10000;
    private float _minY = -10000, _maxY = 10000;

    private double _timeElapsed = 0.0;
    private int _lastMiniBossMinute = 0;
    private bool _bossSpawned = false;

    /// <summary>
    /// Встановлює фізичні межі карти, щоб вороги не з'являлися за її краями.
    /// Зазвичай викликається класом рівня (наприклад, LevelForest) при старті.
    /// </summary>
    /// <param name="minX">Мінімальна координата X (лівий край).</param>
    /// <param name="maxX">Максимальна координата X (правий край).</param>
    /// <param name="minY">Мінімальна координата Y (верхній край).</param>
    /// <param name="maxY">Максимальна координата Y (нижній край).</param>
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = minX;
        _maxX = maxX;
        _minY = minY;
        _maxY = maxY;
        GD.Print($"[Спавнер] Отримав нові кордони: X({_minX} до {_maxX}), Y({_minY} до {_maxY})");
    }

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Знаходить вузол гравця на сцені за групою "Player" та підключає таймер регулярного спавну.
    /// </summary>
    public override void _Ready()
    {
        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = (Node2D)playerNode;
        }

        _spawnTimer = GetNode<Timer>("SpawnTimer");
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
    }

    /// <summary>
    /// Динамічно оновлює складність гри на основі поточного рівня.
    /// Збільшує максимальну кількість ворогів на екрані та зменшує затримку між їх появою.
    /// </summary>
    /// <param name="level">Поточний рівень (або хвиля), від якого розраховується складність.</param>
    public void UpdateDifficulty(int level)
    {
        _maxEnemies = Math.Min(10 + (level * 5), 100);

        float newWaitTime = 1.5f - (level * 0.08f);
        _spawnTimer.WaitTime = Math.Max(newWaitTime, 0.1f);

        GD.Print($"[Спавнер] Новий ліміт: {_maxEnemies}, Кулдаун спавну: {_spawnTimer.WaitTime:F2} сек");
    }

    /// <summary>
    /// Обробник події таймера регулярного спавну (_spawnTimer).
    /// Перевіряє поточну кількість ворогів, розраховує валідну позицію та створює нового ворога,
    /// якщо ліміт ще не досягнуто.
    /// </summary>
    private void OnSpawnTimerTimeout()
    {
        if (_player == null) return;

        int currentEnemies = GetTree().GetNodesInGroup("Enemies").Count;

        if (currentEnemies >= _maxEnemies) return;

        Vector2 finalPosition = GetValidSpawnPosition();

        var enemy = EnemyScene.Instantiate<Enemy>();

        GetParent().AddChild(enemy);
        enemy.GlobalPosition = finalPosition;

        if (HealthMultiplier != 1.0f)
        {
            enemy.BaseHealth = (int)(enemy.BaseHealth * HealthMultiplier);
            enemy._currentHealth = enemy.BaseHealth;
        }
    }

    /// <summary>
    /// Альтернативний метод створення ворога за допомогою випадкового кута навколо гравця.
    /// Додає створеного об'єкта до групи "Enemies".
    /// </summary>
    private void SpawnEnemy()
    {
        float randomAngle = _rng.RandfRange(0, Mathf.Tau);

        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(randomAngle) * SpawnRadius,
            Mathf.Sin(randomAngle) * SpawnRadius
        );

        Vector2 spawnPosition = _player.GlobalPosition + spawnOffset;

        var enemy = EnemyScene.Instantiate<CharacterBody2D>();
        enemy.GlobalPosition = spawnPosition;

        enemy.AddToGroup("Enemies");

        GetParent().AddChild(enemy);
    }

    /// <summary>
    /// Викликається кожен кадр гри. 
    /// Оновлює внутрішній таймер виживання та перевіряє умови для створення особливих ворогів.
    /// </summary>
    /// <param name="delta">Час у секундах з моменту останнього кадру.</param>
    public override void _Process(double delta)
    {
        _timeElapsed += delta;
        CheckSpecialSpawns();
    }

    /// <summary>
    /// Відстежує ігровий час для запуску спеціальних подій:
    /// поява міні-босів кожні 5 хвилин та фінального боса на 20-й хвилині.
    /// </summary>
    private void CheckSpecialSpawns()
    {
        int minutes = (int)(_timeElapsed / 60);

        if (minutes >= 20 && !_bossSpawned)
        {
            SpawnBoss(isFinal: true);
            _bossSpawned = true;
            GD.Print("ГОЛОВНИЙ БОСС ПРИЙШОВ!");
        }
        else if (minutes > 0 && minutes % 5 == 0 && minutes != _lastMiniBossMinute && minutes != 20)
        {
            SpawnBoss(isFinal: false);
            _lastMiniBossMinute = minutes;
            GD.Print($"Міні-босс {minutes}-ї хвилини з'явився!");
        }
    }

    /// <summary>
    /// Створює боса з посиленими характеристиками.
    /// </summary>
    /// <param name="isFinal">
    /// Якщо true — створюється фінальний бос (величезний розмір, червоний колір, максимальне здоров'я).
    /// Якщо false — створюється проміжний міні-бос.
    /// </param>
    private void SpawnBoss(bool isFinal)
    {
        var enemy = EnemyScene.Instantiate<Enemy>();
        enemy.GlobalPosition = GetValidSpawnPosition();

        if (isFinal)
        {
            enemy.Scale = new Vector2(2.0f, 2.0f);
            enemy.BaseHealth = (int)(enemy.BaseHealth * 50 * HealthMultiplier);
            enemy.Modulate = new Color(1, 0.3f, 0.3f);
            enemy.ScoreValue = 100;
        }
        else
        {
            enemy.Scale = new Vector2(1.5f, 1.5f);
            enemy.BaseHealth = (int)(enemy.BaseHealth * 15 * HealthMultiplier);
            enemy.ScoreValue = 20;
        }

        enemy._currentHealth = enemy.BaseHealth;
        GetParent().AddChild(enemy);
    }

    /// <summary>
    /// Розраховує безпечну позицію для спавну об'єкта, враховуючи позицію гравця 
    /// та суворі межі карти (запобігає появі ворогів поза доступною зоною).
    /// </summary>
    /// <returns>Вектор координат (Vector2) для безпечного розміщення об'єкта.</returns>
    private Vector2 GetValidSpawnPosition()
    {
        float angle = (float)GD.RandRange(0, Mathf.Tau);
        float spawnDistance = 700.0f;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;
        Vector2 targetPos = _player.GlobalPosition + offset;

        float margin = 64.0f;

        targetPos.X = Mathf.Clamp(targetPos.X, _minX + margin, _maxX - margin);
        targetPos.Y = Mathf.Clamp(targetPos.Y, _minY + margin, _maxY - margin);

        return targetPos;
    }

    /// <summary>
    /// Спеціальний режим, що прибирає ліміти на кількість ворогів 
    /// та встановлює екстремально коротку затримку спавну (0.05 сек).
    /// </summary>
    public void ActivateHordeMode()
    {
        _maxEnemies = 999999;
        _spawnTimer.WaitTime = 0.05f;
        GD.Print("[Спавнер] РЕЖИМ ОРДИ АКТИВОВАНО! Лімітів немає.");
    }
}