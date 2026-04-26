using Godot;
using System.Collections.Generic;

public partial class UpgradeMenu : Control
{
    private GameManager _gameManager;
    private Texture2D _dummyIcon = GD.Load<Texture2D>("res://icon.svg");

    public override void _Ready()
    {
        _gameManager = GetTree().Root.GetNode<GameManager>("Main/GameManager");
        var hBox = GetNode<HBoxContainer>("Panel/HBoxContainer");

        List<GameManager.UpgradeData> choices = _gameManager.GetUpgradeChoices();

        for (int i = 0; i < 3; i++)
        {
            var cardButton = hBox.GetChild<Button>(i);

            if (i < choices.Count)
            {
                var data = choices[i];

                // --- НОВА ЛОГІКА РІВНІВ ---
                int currentLvl = _gameManager.GetUpgradeLevel(data.Id);
                int nextLvl = currentLvl + 1;

                string levelText;
                if (currentLvl == 0)
                {
                    levelText = "✨ НОВИЙ!"; // Якщо предмета ще немає
                }
                else
                {
                    levelText = $"Рівень: {currentLvl} ➔ {nextLvl}"; // Показуємо прогресію
                }

                // --- ЗАПОВНЕННЯ КАРТКИ ---
                cardButton.GetNode<Label>("VBoxContainer/TitleLabel").Text = data.Title;
                cardButton.GetNode<TextureRect>("VBoxContainer/IconRect").Texture = _dummyIcon;

                // Вставляємо наш красивий текст рівня
                cardButton.GetNode<Label>("VBoxContainer/LevelLabel").Text = levelText;

                // Вставляємо опис
                cardButton.GetNode<Label>("VBoxContainer/DescriptionLabel").Text = data.Description;

                string upgradeId = data.Id;
                cardButton.Pressed += () => OnUpgradeSelected(upgradeId);
            }
            else
            {
                cardButton.Visible = false;
            }
        }
    }

    private void OnUpgradeSelected(string id)
    {
        _gameManager.ApplyUpgrade(id);
        GetTree().Paused = false;
        QueueFree();
    }
}