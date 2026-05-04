using Godot;
using System;

/// <summary>
/// Клас, що керує меню паузи в грі.
/// Відповідає за перехоплення натискання клавіші паузи (Esc), зупинку ігрового часу 
/// та відображення кнопок для продовження або виходу з гри.
/// </summary>
public partial class PauseMenu : Control
{
    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Приховує меню паузи при старті, щоб воно не заважало під час звичайного ігрового процесу.
    /// </summary>
    public override void _Ready()
    {
        Hide();
    }

    /// <summary>
    /// Перехоплює системне введення до того, як воно дійде до ігрових об'єктів.
    /// Відстежує натискання дії "ui_cancel" (зазвичай це клавіша Escape) для перемикання стану паузи.
    /// </summary>
    /// <param name="@event">Об'єкт події введення від гравця (натискання клавіш, клік миші тощо).</param>
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

    /// <summary>
    /// Зупиняє ігровий процес (змінює глобальний стан дерева на Paused) та відображає вікно меню.
    /// </summary>
    private void Pause()
    {
        GetTree().Paused = true;
        Show();
    }

    /// <summary>
    /// Відновлює ігровий процес (знімає гру з паузи) та приховує вікно меню.
    /// </summary>
    private void Unpause()
    {
        GetTree().Paused = false;
        Hide();
    }

    /// <summary>
    /// Обробник події натискання кнопки "Продовжити" (Resume).
    /// Викликає метод <see cref="Unpause"/> для швидкого повернення до ігрового процесу.
    /// </summary>
    public void _on_resume_button_pressed()
    {
        Unpause();
    }

    /// <summary>
    /// Обробник події натискання кнопки "Вийти в меню" (Quit).
    /// Обов'язково знімає гру з паузи перед зміною сцени, щоб уникнути блокування логіки в головному меню, 
    /// після чого завантажує стартову сцену гри.
    /// </summary>
    public void _on_quit_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}