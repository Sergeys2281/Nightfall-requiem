using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    // --- НАЛАШТУВАННЯ ХАРАКТЕРИСТИК ---
    [Export] public float Speed { get; set; } = 50.0f;
    [Export] public float BaseHealth { get; set; } = 20.0f;

    // --- НАЛАШТУВАННЯ АТАКИ ---
    [Export] public int Damage { get; set; } = 5;
    [Export] public float AttackCooldown { get; set; } = 1.0f;

    // --- ЛУТ ---
    [Export] public PackedScene GemScene { get; set; } // Сюди треба перетягнути exp_gem.tscn

    private float _currentHealth;
    private float _timeSinceLastAttack = 0.0f;
    private Player _player;
    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        // 1. Обов'язково додаємо в групу для лімітів спавнера
        AddToGroup("Enemies");

        // 2. Шукаємо гравця
        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = playerNode as Player;
        }

        // 3. Запитуємо у GameManager множник здоров'я
        var gm = GetTree().Root.FindChild("GameManager", true, false) as GameManager;
        float multiplier = gm?.GetEnemyHpMultiplier() ?? 1.0f;

        _currentHealth = BaseHealth * multiplier;

        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (_animatedSprite != null) _animatedSprite.Play("walk");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Додаємо час до таймера атаки незалежно від того, чи ми торкаємося гравця
        _timeSinceLastAttack += (float)delta;

        if (_player != null)
        {
            // --- РУХ ---
            Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * Speed;

            // --- ДОДАЄМО АНІМАЦІЮ ПОВОРОТУ ---
            if (_animatedSprite != null && direction.X != 0)
            {
                _animatedSprite.FlipH = direction.X < 0;
            }

            MoveAndSlide();


            // --- АТАКА ---
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

    // --- ОТРИМАННЯ ШКОДИ ---
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        // орк блимає червоним
        Modulate = new Color(1, 0, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Якщо сцена кристала додана, спавнимо її
        if (GemScene != null)
        {
            var gem = GemScene.Instantiate<Node2D>();

            // Використовуємо CallDeferred для безпечного додавання об'єкта під час розрахунку фізики
            GetParent().CallDeferred("add_child", gem);

            gem.GlobalPosition = GlobalPosition; // Кристал падає там, де помер ворог
        }

        QueueFree();
    }
}