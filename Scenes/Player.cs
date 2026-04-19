using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 150.0f;
    [Export] public PackedScene SlashScene { get; set; }

    [Export] public int MaxHealth { get; set; } = 100;
    private int _currentHealth;
    private ProgressBar _hpBar;

    private AnimatedSprite2D _animatedSprite;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        GetNode<Timer>("AttackTimer").Timeout += OnAttackTimerTimeout;

        AddToGroup("Player");

        _currentHealth = MaxHealth;

        _hpBar = GetTree().Root.GetNodeOrNull<ProgressBar>("Main/UI/HpBar");
        if (_hpBar != null)
        {
            _hpBar.MaxValue = MaxHealth;
            _hpBar.Value = _currentHealth;
        }

        _animatedSprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = direction * Speed;

        if (direction != Vector2.Zero)
        {
            _animatedSprite.Play("walk");
            _animatedSprite.FlipH = direction.X < 0;
        }
        else
        {
            _animatedSprite.Play("idle");
        }

        MoveAndSlide();
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;

        if (_hpBar != null)
        {
            _hpBar.Value = _currentHealth;
        }

        Modulate = new Color(1, 0, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GD.Print("ГРАВЕЦЬ ЗАГИНУВ! ГРУ ЗАКІНЧЕНО!");
        GetTree().ReloadCurrentScene();
    }

    private void OnAttackTimerTimeout()
    {
        if (SlashScene == null) return;

        var slash = SlashScene.Instantiate<SlashEffect>();

        AddChild(slash);

        float offsetX = _animatedSprite.FlipH ? -35.0f : 35.0f;
        slash.Position = new Vector2(offsetX, 0);

        var slashSprite = slash.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        slashSprite.FlipH = _animatedSprite.FlipH;
    }
}