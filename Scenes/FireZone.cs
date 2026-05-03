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
        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

        if (collisionShape != null && collisionShape.Shape is CircleShape2D circle)
        {
            circle.Radius = Radius;
        }

        Timer tickTimer = new Timer();
        tickTimer.WaitTime = 0.5f;
        tickTimer.Autostart = true;
        tickTimer.Timeout += OnTick;
        AddChild(tickTimer);

        GetTree().CreateTimer(Lifespan).Timeout += () => QueueFree();

        QueueRedraw();
    }

    public override void _Draw()
    {
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