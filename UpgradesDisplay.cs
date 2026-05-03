using Godot;
using System.Collections.Generic;

public partial class UpgradesDisplay : VBoxContainer
{
    private Dictionary<string, Label> _upgradeLabels = new Dictionary<string, Label>();

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }

    public void AddOrUpdateUpgrade(string upgradeName, int level)
    {
        if (_upgradeLabels.ContainsKey(upgradeName))
        {
            _upgradeLabels[upgradeName].Text = $"{upgradeName} - {level} рівень";

            _upgradeLabels[upgradeName].Modulate = new Color(0, 1, 0);
            GetTree().CreateTimer(0.3f).Timeout += () =>
            {
                if (IsInstanceValid(_upgradeLabels[upgradeName]))
                    _upgradeLabels[upgradeName].Modulate = new Color(1, 1, 1);
            };
        }
        else
        {
            Label newLabel = new Label();
            newLabel.Text = $"{upgradeName} - {level} рівень";

            newLabel.AddThemeFontSizeOverride("font_size", 18);
            newLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0));
            newLabel.AddThemeConstantOverride("outline_size", 4);

            AddChild(newLabel);
            _upgradeLabels.Add(upgradeName, newLabel);
        }
    }
}