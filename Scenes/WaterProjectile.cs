using Godot;
using System;

public partial class WaterProjectile : Area2D
{
    [Export] public float Speed { get; set; } = 200.0f;
    public int Damage { get; set; } = 7;
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

        float distanceTraveled = GlobalPosition.DistanceTo(_startPosition);

        if (distanceTraveled > MaxDistance)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }
}