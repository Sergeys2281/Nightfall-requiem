using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 80.0f;
    [Export] public int MaxHealth { get; set; } = 3;
    [Export] public PackedScene GemScene { get; set; }
    [Export] public int Damage = 10;
    [Export] public float AttackCooldown = 1.0f;

    private float _timeSinceLastAttack = 0.0f;
    private int _currentHealth;
    private Node2D _player;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;

        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = (Node2D)playerNode;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeSinceLastAttack += (float)delta;

        if (_player != null)
        {
            Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * Speed;

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

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        Modulate = new Color(1, 0, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GemScene != null)
        {
            var gem = GemScene.Instantiate<Node2D>();
            GetParent().CallDeferred("add_child", gem);
            gem.GlobalPosition = GlobalPosition;
        }

        QueueFree();
    }
}