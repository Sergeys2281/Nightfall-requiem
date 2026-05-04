using Godot;

/// <summary>
/// Клас, що відповідає за ефект удару в ближньому бою (наприклад, змах мечем).
/// Створює тимчасову фізичну зону ураження, відтворює візуальну анімацію 
/// та автоматично знищується відразу після її завершення.
/// </summary>
public partial class SlashEffect : Area2D
{
    /// <summary>
    /// Кількість шкоди, яку отримує ворог при потраплянні в зону удару.
    /// </summary>
    public int Damage = 10;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла.
    /// Запускає базову анімацію удару, підключає подію її завершення до самознищення об'єкта (<see cref="Node.QueueFree"/>),
    /// а також активує відстеження фізичних зіткнень з іншими тілами.
    /// </summary>
    public override void _Ready()
    {
        var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        anim.Play("default");

        anim.AnimationFinished += QueueFree;

        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Обробник події входження фізичного об'єкта в зону удару.
    /// Перевіряє, чи є об'єкт ворогом (клас Enemy), і якщо так — миттєво завдає йому шкоди.
    /// </summary>
    /// <param name="body">Фізичне тіло (Node2D), яке потрапило в зону ураження.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }
}