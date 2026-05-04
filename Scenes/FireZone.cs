using Godot;
using System;

/// <summary>
/// Клас, що реалізує механіку вогняної зони (AoE - Area of Effect), яка наносить періодичну шкоду ворогам.
/// Зазвичай створюється після вибуху або знищення вогняного снаряда (наприклад, <see cref="FireProjectile"/>).
/// </summary>
public partial class FireZone : Area2D
{
    /// <summary>
    /// Радіус дії вогняної зони. Визначає як візуальний розмір відмальованого кола, 
    /// так і фізичну площу ураження (колайдера).
    /// </summary>
    [Export] public float Radius { get; set; } = 50.0f;

    /// <summary>
    /// Колір зони, що відмальовується на екрані (за замовчуванням — напівпрозорий червоний).
    /// </summary>
    [Export] public Color ZoneColor { get; set; } = new Color(1, 0, 0, 0.4f);

    /// <summary>
    /// Кількість шкоди, яку зона наносить ворогам за один тік (кожні 0.5 секунд).
    /// </summary>
    public int Damage = 5;

    /// <summary>
    /// Загальний час існування зони (у секундах) до її автоматичного зникнення з ігрового світу.
    /// </summary>
    public float Lifespan = 1.0f;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Синхронізує розмір фізичного колайдера з радіусом, програмно створює та запускає таймер 
    /// періодичного нанесення шкоди, а також встановлює ліміт часу життя об'єкта.
    /// </summary>
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

    /// <summary>
    /// Вбудований метод Godot для кастомного малювання (CanvasItem).
    /// Відмальовує візуальне відображення вогняної зони у вигляді кола заданого радіусу та кольору.
    /// </summary>
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius, ZoneColor);
    }

    /// <summary>
    /// Обробник події таймера (спрацьовує кожні 0.5 секунд).
    /// Перевіряє всі фізичні тіла, що наразі перетинаються із зоною, 
    /// і наносить шкоду тим об'єктам, які є ворогами (належать до класу Enemy).
    /// </summary>
    private void OnTick()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Enemy enemy) enemy.TakeDamage(Damage);
        }
    }
}