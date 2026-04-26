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
        // Запам'ятовуємо початкову точку при спавні
        _startPosition = GlobalPosition;

        // Підключаємо подію зіткнення
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Летимо вперед
        GlobalPosition += Transform.X * Speed * (float)delta;

        // Рахуємо дистанцію від точки старту до поточної позиції
        float distanceTraveled = GlobalPosition.DistanceTo(_startPosition);

        // Якщо пролетіли занадто далеко, зникаємо
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