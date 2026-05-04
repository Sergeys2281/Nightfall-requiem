using Godot;
using System;

/// <summary>
/// Клас, що керує логікою стріли (Arrow).
/// Стріла летить по прямій і знищується при першому ж зіткненні з ворогом, 
/// завдаючи йому значної шкоди. Також має ліміт дальності польоту.
/// </summary>
public partial class Arrow : Area2D
{
    /// <summary>
    /// Швидкість польоту стріли у пікселях на секунду.
    /// </summary>
    [Export] public float Speed { get; set; } = 300.0f;

    /// <summary>
    /// Кількість шкоди, яку стріла наносить ворогу при влучанні.
    /// </summary>
    public int Damage { get; set; } = 15;

    /// <summary>
    /// Максимальна відстань (у пікселях), яку може пролетіти стріла перед тим, як зникне.
    /// Запобігає накопиченню об'єктів у пам'яті (memory leaks), якщо стріла летить у порожнечу.
    /// </summary>
    public float MaxDistance { get; set; } = 2500.0f;

    private Vector2 _startPosition;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Запам'ятовує початкову позицію для подальшого розрахунку дальності польоту 
    /// та підключає обробник події зіткнення з іншими тілами.
    /// </summary>
    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Обробляється кожен фізичний кадр гри.
    /// Відповідає за переміщення стріли вперед та її видалення, якщо подолана максимальна дистанція.
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Transform.X * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(_startPosition) > MaxDistance)
        {
            QueueFree();
        }
    }

    /// <summary>
    /// Обробник події входження фізичного об'єкта в зону колайдера стріли.
    /// Якщо тіло є ворогом (клас Enemy), стріла завдає йому шкоди та одразу знищується.
    /// На відміну від пробивних снарядів, стріла вражає суворо одну ціль.
    /// </summary>
    /// <param name="body">Фізичне тіло (Node2D), з яким відбулося зіткнення.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
            QueueFree();
        }
    }
}