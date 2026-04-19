using Godot;
using System;

public partial class ExpGem : Area2D
{
    [Export] public int ExpValue { get; set; } = 10;
    [Export] public float MoveSpeed { get; set; } = 300.0f;

    private bool _isMovingToPlayer = false;
    private Node2D _player;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

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

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            _player = body;
            _isMovingToPlayer = true;
        }
    }

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