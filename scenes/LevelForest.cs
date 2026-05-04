using Godot;
using System;

/// <summary>
/// Клас, що керує логікою ініціалізації рівня "Ліс" (Forest).
/// Відповідає за динамічний розрахунок розмірів ігрової карти на основі плиток (TileMapLayer)
/// та встановлення відповідних фізичних обмежень для камери гравця та спавнера ворогів.
/// </summary>
public partial class LevelForest : Node2D
{
    /// <summary>
    /// Викликається рушієм Godot після того, як вузол та всі його дочірні елементи завантажені у дерево сцени.
    /// Знаходить шар землі (Ground), вираховує його реальні межі у пікселях, 
    /// після чого обмежує рух головної камери та передає ці координати системі спавну, 
    /// щоб вороги не з'являлися за межами карти.
    /// </summary>
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