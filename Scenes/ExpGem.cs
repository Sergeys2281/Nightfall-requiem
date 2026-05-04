using Godot;
using System;

/// <summary>
/// Клас, що керує логікою кристала досвіду (ExpGem).
/// Кристали випадають з переможених ворогів. Коли гравець підходить достатньо близько, 
/// кристал активує ефект "магніту", плавно летить до гравця і після досягнення передає йому очки досвіду.
/// </summary>
public partial class ExpGem : Area2D
{
    /// <summary>
    /// Кількість очок досвіду, яку гравець отримає при поглинанні цього кристала.
    /// </summary>
    [Export] public int ExpValue { get; set; } = 10;

    /// <summary>
    /// Швидкість польоту кристала (у пікселях на секунду) до гравця після активації "магніту".
    /// </summary>
    [Export] public float MoveSpeed { get; set; } = 300.0f;

    private bool _isMovingToPlayer = false;
    private Node2D _player;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Підключає обробник подій для відстеження моменту, коли гравець входить у зону дії (радіус підбирання).
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Обробляється кожен фізичний кадр гри.
    /// Якщо кристал зафіксував гравця і перейшов у стан польоту, він наближається до нього.
    /// Коли дистанція стає меншою за 15 пікселів, кристал поглинається.
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (_isMovingToPlayer && _player != null)
        {
            GlobalPosition = GlobalPosition.MoveToward(_player.GlobalPosition, MoveSpeed * (float)delta);

            if (GlobalPosition.DistanceTo(_player.GlobalPosition) < 15.0f)
            {
                Consume();
            }
        }
    }

    /// <summary>
    /// Обробник події входження фізичного об'єкта в зону підбирання кристала.
    /// Якщо об'єкт належить до групи "Player", кристал запам'ятовує ціль та активує режим польоту.
    /// </summary>
    /// <param name="body">Фізичне тіло (Node2D), з яким відбулося зіткнення.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            _player = body;
            _isMovingToPlayer = true;
        }
    }

    /// <summary>
    /// Логіка поглинання кристала.
    /// Знаходить глобальний менеджер гри (<see cref="GameManager"/>), передає йому значення досвіду (<see cref="ExpValue"/>)
    /// та видаляє об'єкт кристала з ігрового світу.
    /// </summary>
    private void Consume()
    {
        var gameManagerNode = GetTree().GetFirstNodeInGroup("GameManager");

        if (gameManagerNode is GameManager manager)
        {
            manager.AddExperience(ExpValue);
            GD.Print("Досвід успішно передано!");
        }
        else
        {
            GD.Print("ПОМИЛКА: Не знайдено GameManager!");
        }

        QueueFree();
    }
}