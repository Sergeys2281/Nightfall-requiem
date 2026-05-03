using Godot;
using System;

public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (GetTree().Paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        GetTree().Paused = true;
        Show();
    }

    private void Unpause()
    {
        GetTree().Paused = false;
        Hide();
    }


    public void _on_resume_button_pressed()
    {
        Unpause();
    }

    public void _on_quit_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}