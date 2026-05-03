using Godot;
using System;

public partial class Aura : Area2D
{
    public int Damage = 5;

    public override void _Ready()
    {
        Visible = false;
        Monitoring = false;

        var timer = GetNode<Timer>("TickTimer");
        timer.Timeout += OnTick;
    }

    private void OnTick()
    {
        if (!Monitoring) return;

        var overlappingBodies = GetOverlappingBodies();

        foreach (var body in overlappingBodies)
        {
            if (body is Enemy enemy)
            {
                enemy.TakeDamage(Damage);
            }
        }
    }
}