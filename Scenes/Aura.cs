using Godot;
using System;

public partial class Aura : Area2D
{
    public int Damage = 5;

    public override void _Ready()
    {
        // На початку гри аура невидима і не наносить урону
        Visible = false;
        Monitoring = false;

        // Підключаємо таймер урону
        var timer = GetNode<Timer>("TickTimer");
        timer.Timeout += OnTick;
    }

    private void OnTick()
    {
        // Якщо аура ще не розблокована, нічого не робимо
        if (!Monitoring) return;

        // Отримуємо всіх, хто зараз стоїть в аурі
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