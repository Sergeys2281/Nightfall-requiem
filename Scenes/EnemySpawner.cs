using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public float SpawnRadius { get; set; } = 600.0f;

    private Node2D _player;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        var playerNode = GetTree().GetFirstNodeInGroup("Player");
        if (playerNode != null)
        {
            _player = (Node2D)playerNode;
        }

        GetNode<Timer>("SpawnTimer").Timeout += OnSpawnTimerTimeout;
    }

    private void OnSpawnTimerTimeout()
    {
        if (_player == null || EnemyScene == null) return;

        float randomAngle = _rng.RandfRange(0, Mathf.Tau);

        Vector2 spawnOffset = new Vector2(
            Mathf.Cos(randomAngle) * SpawnRadius,
            Mathf.Sin(randomAngle) * SpawnRadius
        );

        Vector2 spawnPosition = _player.GlobalPosition + spawnOffset;

        var enemy = EnemyScene.Instantiate<CharacterBody2D>();
        enemy.GlobalPosition = spawnPosition;

        GetParent().AddChild(enemy);
    }
}