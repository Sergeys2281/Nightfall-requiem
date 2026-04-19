using Godot;

public partial class SlashEffect : Area2D
{
    public int Damage = 1;

    public override void _Ready()
    {
        var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Як тільки з'являємося — програємо анімацію
        anim.Play("default");

        // Коли анімація закінчується — автоматично видаляємо ефект з пам'яті
        anim.AnimationFinished += QueueFree;

        // Підключаємо вбудований сигнал Godot: якщо хтось торкається цієї зони
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Якщо той, хто торкнувся, є ворогом — наносимо шкоду
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }
}