using Godot;
using System;

public partial class FireZone : Area2D
{
    [Export] public float Radius { get; set; } = 50.0f;
    [Export] public Color ZoneColor { get; set; } = new Color(1, 0, 0, 0.4f);

    public int Damage = 5;
    public float Lifespan = 1.0f;

    public override void _Ready()
    {
        // 1. СИНХРОНІЗАЦІЯ З КОЛІЗІЄЮ
        // Шукаємо вузол колізії
        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        // Перевіряємо, чи він існує і чи це справді коло (CircleShape2D)
        if (collisionShape != null && collisionShape.Shape is CircleShape2D circle)
        {
            // Призначаємо фізичний радіус рівним нашому візуальному
            circle.Radius = Radius;
        }

        // 2. ТАЙМЕРИ
        Timer tickTimer = new Timer();
        tickTimer.WaitTime = 0.5f;
        tickTimer.Autostart = true;
        tickTimer.Timeout += OnTick;
        AddChild(tickTimer);

        GetTree().CreateTimer(Lifespan).Timeout += () => QueueFree();

        // 3. ВІЗУАЛЬНЕ ОНОВЛЕННЯ
        QueueRedraw();
    }

    public override void _Draw()
    {
        // Малюємо коло, використовуючи той самий Radius
        DrawCircle(Vector2.Zero, Radius, ZoneColor);
    }

    private void OnTick()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Enemy enemy) enemy.TakeDamage(Damage);
        }
    }
}