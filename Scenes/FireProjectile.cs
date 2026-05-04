using Godot;
using System;

/// <summary>
/// Клас, що керує логікою вогняного снаряда (FireProjectile).
/// Снаряд летить по прямій і при зіткненні з ворогом або досягненні максимальної дистанції 
/// вибухає, створюючи вогняну калюжу (FireZone), яка наносить шкоду по площі.
/// </summary>
public partial class FireProjectile : Area2D
{
    /// <summary>
    /// Швидкість польоту снаряда у пікселях на секунду.
    /// </summary>
    public float Speed = 150.0f;

    /// <summary>
    /// Кількість шкоди, яка буде передана створеній вогняній зоні.
    /// </summary>
    public int ZoneDamage = 5;

    /// <summary>
    /// Час існування (у секундах), який буде передано створеній вогняній зоні.
    /// </summary>
    public float ZoneLifespan = 1.0f;

    /// <summary>
    /// Візуальний та фізичний масштаб, який буде застосовано до створеної вогняної зони.
    /// </summary>
    public Vector2 ZoneScale = new Vector2(1, 1);

    /// <summary>
    /// Посилання на шаблон сцени вогняної зони (FireZone), яка з'явиться після знищення снаряда.
    /// </summary>
    [Export] public PackedScene FireZoneScene { get; set; }

    private bool _isDestroyed = false;
    private float _distanceTraveled = 0.0f;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Підключає обробник подій для відстеження фізичних зіткнень з іншими тілами (наприклад, ворогами).
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Обробляється кожен фізичний кадр гри. 
    /// Відповідає за переміщення снаряда вперед на основі його швидкості.
    /// Також відстежує пройдену відстань: якщо снаряд пролітає більше 1500 пікселів, він автоматично знищується.
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
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

    /// <summary>
    /// Обробник події входження фізичного об'єкта в зону снаряда.
    /// Якщо об'єкт є ворогом (клас Enemy), снаряд припиняє політ та активує спавн вогняної зони.
    /// </summary>
    /// <param name="body">Фізичне тіло (Node2D), з яким відбулося зіткнення.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy && !_isDestroyed)
        {
            _isDestroyed = true;
            Callable.From(SpawnZone).CallDeferred();
        }
    }

    /// <summary>
    /// Створює вогняну зону (FireZone) на місці знищення снаряда.
    /// Передає їй розраховані раніше характеристики (шкоду, час життя, розмір),
    /// додає зону до ігрового світу та видаляє сам снаряд з пам'яті.
    /// </summary>
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