using Godot;
using System.Collections.Generic;

public partial class UpgradeMenu : Control
{
    private GameManager _gameManager;
    private Texture2D _dummyIcon = GD.Load<Texture2D>("res://icon.svg");

    private Dictionary<string, string> _iconPaths = new Dictionary<string, string>
    {
        { "Sword", "res://assets/sword_icon.png" },
        { "Bow", "res://assets/bow_icon.png" },
        { "Aura", "res://assets/aura_icon.png" },
        { "Staff", "res://assets/waterstaff_icon.png" },
        { "FireStaff", "res://assets/firestaff_icon.png" },
        { "MaxHP", "res://assets/maxhp_icon.png" },
        { "Speed", "res://assets/movespeed_icon.png" },
        { "Damage", "res://assets/strenght_icon.png" },
        { "Armor", "res://assets/armor_icon.png" },
        { "Regen", "res://assets/regen_icon.png" },
        { "Cooldown", "res://assets/attackspeed_icon.png" }
    };

    public override void _Ready()
    {
        _gameManager = GetTree().Root.GetNode<GameManager>("Main/GameManager");
        var hBox = GetNode<HBoxContainer>("Panel/HBoxContainer");

        List<GameManager.UpgradeData> choices = _gameManager.GetUpgradeChoices();

        var levelUpSound = GetNodeOrNull<AudioStreamPlayer>("LevelUpSound");
        if (levelUpSound != null) levelUpSound.Play();

        for (int i = 0; i < 3; i++)
        {
            var cardButton = hBox.GetChild<Button>(i);

            if (i < choices.Count)
            {
                var data = choices[i];

                int currentLvl = _gameManager.GetUpgradeLevel(data.Id);
                int nextLvl = currentLvl + 1;

                string levelText;
                if (currentLvl == 0)
                {
                    levelText = "✨ НОВИЙ!";
                }
                else
                {
                    levelText = $"Рівень: {currentLvl} ➔ {nextLvl}";
                }

                Texture2D iconTexture = _dummyIcon;

                if (_iconPaths.ContainsKey(data.Id))
                {
                    iconTexture = GD.Load<Texture2D>(_iconPaths[data.Id]);
                }

                cardButton.GetNode<Label>("VBoxContainer/TitleLabel").Text = data.Title;

                var iconRect = cardButton.GetNode<TextureRect>("VBoxContainer/IconRect");
                iconRect.Texture = iconTexture;

                iconRect.CustomMinimumSize = new Vector2(64, 64);
                iconRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
                iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

                iconRect.TextureFilter = Control.TextureFilterEnum.Nearest;

                cardButton.GetNode<Label>("VBoxContainer/LevelLabel").Text = levelText;
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