using Godot;
using System;

/// <summary>
/// Клас, що керує логікою водяного снаряда (WaterProjectile).
/// Снаряд летить по прямій задану дистанцію. Він має ефект "пробивання" (piercing): 
/// проходить крізь ворогів, наносячи їм шкоду, і зникає лише після подолання максимальної відстані.
/// </summary>
public partial class WaterProjectile : Area2D
{
    /// <summary>
    /// Швидкість польоту снаряда у пікселях на секунду.
    /// </summary>
    [Export] public float Speed { get; set; } = 200.0f;

    /// <summary>
    /// Кількість шкоди, яку снаряд наносить кожному ворогу, крізь якого пролітає.
    /// </summary>
    public int Damage { get; set; } = 7;

    /// <summary>
    /// Максимальна відстань (у пікселях), яку може пролетіти снаряд перед тим, як зникне.
    /// Захищає гру від витоків пам'яті (memory leaks), коли снаряди летять у нескінченність.
    /// </summary>
    public float MaxDistance { get; set; } = 2500.0f;

    private Vector2 _startPosition;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Запам'ятовує стартову координату для подальшого розрахунку пройденої дистанції 
    /// та підключає обробник подій зіткнення.
    /// </summary>
    public override void _Ready()
    {
        _startPosition = GlobalPosition;

        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Обробляється кожен фізичний кадр. 
    /// Відповідає за переміщення снаряда вперед та знищення об'єкта, якщо він перевищив ліміт дальності польоту.
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Transform.X * Speed * (float)delta;

        float distanceTraveled = GlobalPosition.DistanceTo(_startPosition);

        if (distanceTraveled > MaxDistance)
        {
            QueueFree();
        }
    }

    /// <summary>
    /// Обробник події входження фізичного об'єкта в зону снаряда.
    /// Якщо об'єкт є ворогом (клас Enemy), наносить йому шкоду. 
    /// Об'єкт снаряда при цьому не знищується, що дозволяє вражати кілька цілей поспіль.
    /// </summary>
    /// <param name="body">Фізичне тіло (Node2D), з яким відбулося зіткнення.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }
}