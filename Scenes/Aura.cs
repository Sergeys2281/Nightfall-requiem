using Godot;
using System;

/// <summary>
/// Клас, що реалізує механіку магічної аури навколо гравця.
/// Аура наносить періодичну шкоду всім ворогам, які знаходяться в її радіусі.
/// </summary>
public partial class Aura : Area2D
{
    /// <summary>
    /// Кількість шкоди, яку аура наносить ворогам за один тік (спрацьовування таймера).
    /// Це значення може динамічно змінюватися при отриманні відповідних поліпшень.
    /// </summary>
    public int Damage = 5;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// За замовчуванням приховує ауру та вимикає її фізичний прорахунок (Monitoring), 
    /// оскільки гравець ще не отримав це поліпшення на початку гри.
    /// Також підключає подію таймера для періодичного нанесення шкоди.
    /// </summary>
    public override void _Ready()
    {
        Visible = false;
        Monitoring = false;

        var timer = GetNode<Timer>("TickTimer");
        timer.Timeout += OnTick;
    }

    /// <summary>
    /// Обробник події таймера (TickTimer).
    /// Перевіряє всіх фізичних тіл, що наразі перетинаються з зоною аури, 
    /// і наносить шкоду тим, хто є ворогами (належить до класу Enemy).
    /// </summary>
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