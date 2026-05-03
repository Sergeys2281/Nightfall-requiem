using Godot;
using System;

public partial class FireProjectile : Area2D
{
    public float Speed = 150.0f;
    public int ZoneDamage = 5;
    public float ZoneLifespan = 1.0f;
    public Vector2 ZoneScale = new Vector2(1, 1);

    [Export] public PackedScene FireZoneScene { get; set; }

    private bool _isDestroyed = false;

    private float _distanceTraveled = 0.0f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDestroyed) return;

        float step = Speed * (float)delta;

        GlobalPosition += Transform.X * step;

        _distanceTraveled += step;

        if (_distanceTraveled > 1500.0f)
        {
            _isDestroyed = true;
            Callable.From(SpawnZone).CallDeferred();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy && !_isDestroyed)
        {
            _isDestroyed = true;
            Callable.From(SpawnZone).CallDeferred();
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

            GetTree().CurrentScene.AddChild(zone);
        }

        QueueFree();
    }
}