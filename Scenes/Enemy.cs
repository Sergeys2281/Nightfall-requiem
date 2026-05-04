using Godot;
using System;

/// <summary>
/// Клас, що керує базовою логікою ворогів у грі.
/// Відповідає за переслідування гравця, нанесення шкоди при фізичному зіткненні, 
/// отримання шкоди та смерть із випаданням кристалів досвіду.
/// </summary>
public partial class Enemy : CharacterBody2D
{
    /// <summary>Швидкість пересування ворога у пікселях на секунду.</summary>
    [Export] public float Speed { get; set; } = 50.0f;

    /// <summary>Базовий показник здоров'я ворога до застосування множників складності.</summary>
    [Export] public float BaseHealth { get; set; } = 20.0f;

    /// <summary>Кількість шкоди, яку ворог завдає гравцю при зіткненні.</summary>
    [Export] public int Damage { get; set; } = 5;

    /// <summary>Затримка (у секундах) між атаками ворога по гравцю.</summary>
    [Export] public float AttackCooldown { get; set; } = 1.0f;

    /// <summary>Шаблон сцени кристала досвіду (ExpGem), який створюється після смерті ворога.</summary>
    [Export] public PackedScene GemScene { get; set; }

    public float _currentHealth;
    private float _timeSinceLastAttack = 0.0f;
    private Player _player;
    private AnimatedSprite2D _animatedSprite;

    /// <summary>Кількість очок, що додаються до ігрового рахунку при вбивстві цього ворога.</summary>
    public int ScoreValue = 1;

    private Color _baseColor;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Додає ворога до групи "Enemies", знаходить гравця на сцені, 
    /// розраховує фінальне здоров'я на основі поточної складності у <see cref="GameManager"/> та запускає анімацію.
    /// </summary>
    public override void _Ready()
    {
        _baseColor = Modulate;

        AddToGroup("Enemies");

        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = playerNode as Player;
        }

        var gm = GetTree().Root.FindChild("GameManager", true, false) as GameManager;
        float multiplier = gm?.GetEnemyHpMultiplier() ?? 1.0f;

        _currentHealth = BaseHealth * multiplier;

        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (_animatedSprite != null) _animatedSprite.Play("walk");
    }

    /// <summary>
    /// Обробляється кожен фізичний кадр гри.
    /// Забезпечує рух ворога в напрямку гравця, розворот спрайту залежно від вектора руху 
    /// та обробку зіткнень для нанесення шкоди гравцю (з урахуванням часу перезарядки атаки).
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;

        if (_player != null)
        {
            Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * Speed;

            if (_animatedSprite != null && direction.X != 0)
            {
                _animatedSprite.FlipH = direction.X < 0;
            }

            MoveAndSlide();

            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);

                if (collision.GetCollider() is Player player)
                {
                    if (_timeSinceLastAttack >= AttackCooldown)
                    {
                        player.TakeDamage(Damage);
                        _timeSinceLastAttack = 0.0f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Завдає шкоди ворогу та активує візуальний ефект спалаху (короткочасне підсвічування білим).
    /// Якщо здоров'я падає до нуля або нижче, викликає метод знищення об'єкта.
    /// </summary>
    /// <param name="damage">Кількість отриманої шкоди від атак гравця.</param>
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        Modulate = new Color(2, 2, 2);

        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = _baseColor;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Обробляє логіку смерті ворога: додає очки до загального рахунку, 
    /// створює кристал досвіду на місці своєї загибелі та видаляє об'єкт ворога з ігрового світу.
    /// </summary>
    private void Die()
    {
        GameManager.CurrentScore += ScoreValue;
        if (GemScene != null)
        {
            var gem = GemScene.Instantiate<Node2D>();

            GetParent().CallDeferred("add_child", gem);

            gem.GlobalPosition = GlobalPosition;
        }

        QueueFree();
    }
}