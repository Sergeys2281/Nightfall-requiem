using Godot;
using System;

public partial class LevelForest : Node2D
{
    public override void _Ready()
    {
        var groundLayer = GetNodeOrNull<TileMapLayer>("Ground");
        if (groundLayer == null) return;

        Rect2I usedRect = groundLayer.GetUsedRect();
        int tileSize = groundLayer.TileSet.TileSize.X;

        int left = usedRect.Position.X * tileSize;
        int top = usedRect.Position.Y * tileSize;
        int right = usedRect.End.X * tileSize;
        int bottom = usedRect.End.Y * tileSize;

        var camera = GetTree().GetFirstNodeInGroup("MainCamera") as Camera2D;

        if (camera != null)
        {
            camera.LimitLeft = left;
            camera.LimitTop = top;
            camera.LimitRight = right;
            camera.LimitBottom = bottom;
            GD.Print("[Level] Камеру знайдено через групу, ліміти встановлено.");
        }
        else
        {
            GD.PrintErr("Не можу знайти камеру в групі 'MainCamera'!");
        }

        var spawner = GetNodeOrNull<EnemySpawner>("EnemySpawner");
        if (spawner != null)
        {
            spawner.SetBounds(left, right, top, bottom);
        }
    }
}