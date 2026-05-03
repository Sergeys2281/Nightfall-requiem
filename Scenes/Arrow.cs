using Godot;
using System;

public partial class Arrow : Area2D
{
    [Export] public float Speed { get; set; } = 300.0f;
    public int Damage { get; set; } = 15;
    public float MaxDistance { get; set; } = 2500.0f;

    private Vector2 _startPosition;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Transform.X * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(_startPosition) > MaxDistance)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
            QueueFree();
        }
    }
}