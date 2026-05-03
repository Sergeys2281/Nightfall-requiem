using Godot;

public partial class SlashEffect : Area2D
{
    public int Damage = 10;

    public override void _Ready()
    {
        var anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        anim.Play("default");

        anim.AnimationFinished += QueueFree;

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Наносимо шкоду
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }
}