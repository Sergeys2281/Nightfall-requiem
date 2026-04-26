using Godot;
using System;

public partial class FireProjectile : Area2D
{
    public float Speed = 150.0f;
    public int ZoneDamage = 5;
    public float ZoneLifespan = 1.0f;
    public Vector2 ZoneScale = new Vector2(1, 1);

    [Export] public PackedScene FireZoneScene { get; set; }

    private Vector2 _startPos;
    private bool _isDestroyed = false; // ЗАПОБІЖНИК ВІД СПАМУ

    public override void _Ready()
    {
        _startPos = GlobalPosition;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDestroyed) return; // Якщо вже врізалися, просто чекаємо видалення

        GlobalPosition += Transform.X * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(_startPos) > 1500.0f)
        {
            // Викликаємо відкладено, щоб не зламати рушій
            CallDeferred(MethodName.SpawnZone);
            _isDestroyed = true;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        // Якщо це ворог і ми ще не знищені
        if (body is Enemy && !_isDestroyed)
        {
            _isDestroyed = true; // Блокуємо всі наступні спрацьовування
            CallDeferred(MethodName.SpawnZone);
        }
    }

    private void SpawnZone()
    {
        if (FireZoneScene != null)
        {
            var zone = FireZoneScene.Instantiate<FireZone>();

            zone.GlobalPosition = GlobalPosition;
            zone.Damage = ZoneDamage;
            zone.Lifespan = ZoneLifespan;
            zone.Scale = ZoneScale;

            // Додаємо калюжу туди ж, де знаходиться сам снаряд
            GetParent().AddChild(zone);
        }

        QueueFree();
    }
}