using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 50.0f;
    [Export] public float BaseHealth { get; set; } = 20.0f;

    [Export] public int Damage { get; set; } = 5;
    [Export] public float AttackCooldown { get; set; } = 1.0f;

    [Export] public PackedScene GemScene { get; set; }


    public float _currentHealth;
    private float _timeSinceLastAttack = 0.0f;
    private Player _player;
    private AnimatedSprite2D _animatedSprite;

    public int ScoreValue = 1;

    private Color _baseColor;

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

    // --- ОТРИМАННЯ ШКОДИ ---
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