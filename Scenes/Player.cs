using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 150.0f;
    [Export] public PackedScene SlashScene { get; set; }

    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int BonusDamage { get; set; } = 0;
    [Export] public PackedScene ArrowScene { get; set; }
    [Export] public PackedScene StaffProjectileScene { get; set; }
    [Export] public PackedScene FireProjectileScene { get; set; }

    private int _currentHealth = 100;
    private ProgressBar _hpBar;

    private AnimatedSprite2D _animatedSprite;

    public int Armor { get; set; } = 0;
    private Timer _regenTimer;


    public int MaxHp = 100;

    private AudioStreamPlayer _swordSound;
    private AudioStreamPlayer _damageSound;
    private AudioStreamPlayer _bowSound;
    private AudioStreamPlayer _firestaffSound;
    private AudioStreamPlayer _waterstaffSound;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        GetNode<Timer>("AttackTimer").Timeout += OnAttackTimerTimeout;

        AddToGroup("Player");

        _hpBar = GetNodeOrNull<ProgressBar>("HpBar");

        if (_hpBar != null)
        {
            _hpBar.MaxValue = MaxHp;
            _hpBar.Value = _currentHealth;
        }

        _animatedSprite.Play("idle");

        _regenTimer = new Timer();
        _regenTimer.WaitTime = 5.0f;
        _regenTimer.Autostart = true;
        _regenTimer.Timeout += OnRegenTimerTimeout;
        AddChild(_regenTimer);

        Timer bowTimer = new Timer();
        bowTimer.WaitTime = 1.0f;
        bowTimer.Autostart = true;
        bowTimer.Timeout += OnBowTimerTimeout;
        AddChild(bowTimer);

        Timer staffTimer = new Timer();
        staffTimer.WaitTime = 1.2f;
        staffTimer.Autostart = true;
        staffTimer.Timeout += OnStaffTimerTimeout;
        AddChild(staffTimer);

        Timer fireTimer = new Timer();
        fireTimer.WaitTime = 1.5f;
        fireTimer.Autostart = true;
        fireTimer.Timeout += OnFireTimerTimeout;
        AddChild(fireTimer);

        _swordSound = GetNodeOrNull<AudioStreamPlayer>("SwordSound");
        _damageSound = GetNodeOrNull<AudioStreamPlayer>("DamageSound");
        _bowSound = GetNodeOrNull<AudioStreamPlayer>("BowSound");
        _firestaffSound = GetNodeOrNull<AudioStreamPlayer>("FireStaffSound");
        _waterstaffSound = GetNodeOrNull<AudioStreamPlayer>("WaterStaffSound");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = direction * Speed;

        if (direction != Vector2.Zero)
        {
            _animatedSprite.Play("walk");

            if (direction.X != 0)
            {
                _animatedSprite.FlipH = direction.X < 0;
            }
        }
        else
        {
            _animatedSprite.Play("idle");
        }

        MoveAndSlide();
    }

    public void TakeDamage(int amount)
    {
        int finalDamage = Math.Max(1, amount - Armor);

        _currentHealth -= finalDamage;

        if (_hpBar != null) _hpBar.Value = _currentHealth;

        if (_damageSound != null) _damageSound.Play();

        Modulate = new Color(1, 0, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;

        if (_hpBar != null) _hpBar.Value = _currentHealth;

        Modulate = new Color(0, 1, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);
    }

    public void Die()
    {

        var rawNode = GetTree().CurrentScene.GetNodeOrNull("UI/GameOverMenu");

        if (rawNode == null)
        {
            return;
        }


        var gameOverMenu = rawNode as GameOverMenu;

        if (gameOverMenu != null)
        {
            gameOverMenu.ShowGameOver();
        }
        else
        {
        }
    }

    private void OnAttackTimerTimeout()
    {
        if (SlashScene == null) return;

        var gm = GetTree().Root.GetNode<GameManager>("Main/GameManager");
        var slash = SlashScene.Instantiate<SlashEffect>();

        if (_swordSound != null) _swordSound.Play();

        float baseSwordDamage = 10.0f;
        int swordLevel = gm.GetUpgradeLevel("Sword");
        float swordLevelBonus = (swordLevel - 1) * 5.0f;

        float strengthMult = gm.GetStrengthMultiplier();

        slash.Damage = (int)((baseSwordDamage + swordLevelBonus) * strengthMult);

        float radiusScale = 1.0f + (swordLevel - 1) * 0.1f;
        slash.Scale = new Vector2(radiusScale, radiusScale);

        AddChild(slash);
        float offsetX = _animatedSprite.FlipH ? -35.0f * radiusScale : 35.0f * radiusScale;
        slash.Position = new Vector2(offsetX, 0);

        var slashSprite = slash.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        slashSprite.FlipH = _animatedSprite.FlipH;
    }
    private void OnRegenTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm != null)
        {
            int regenLevel = gm.GetUpgradeLevel("Regen");
            if (regenLevel > 0)
            {
                Heal(regenLevel);
            }
        }
    }

    public void UpdateAttackSpeed()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm != null)
        {
            int cdLevel = gm.GetUpgradeLevel("Cooldown");

            float newWaitTime = 1.2f - (cdLevel * 0.2f);

            newWaitTime = Math.Max(newWaitTime, 0.2f);

            GetNode<Timer>("AttackTimer").WaitTime = newWaitTime;
            GD.Print($"[Player] Новий час перезарядки: {newWaitTime:F2} сек");
        }
    }

    private void OnBowTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null) return;

        int bowLevel = gm.GetUpgradeLevel("Bow");

        if (_bowSound != null) _bowSound.Play();

        if (bowLevel <= 0 || ArrowScene == null) return;

        int arrowCount = bowLevel;

        float baseArrowDamage = 15.0f;
        float levelMultiplier = 1.0f + (bowLevel * 0.05f);
        float strengthMult = gm.GetStrengthMultiplier();
        int finalDamage = (int)(baseArrowDamage * levelMultiplier * strengthMult);

        float randomAngle = (float)GD.RandRange(0, Mathf.Tau);

        for (int i = 0; i < arrowCount; i++)
        {
            float angleOffset = (i - (arrowCount - 1) / 2.0f) * Mathf.DegToRad(15);
            float currentAngle = randomAngle + angleOffset;

            var arrow = ArrowScene.Instantiate<Arrow>();
            GetParent().AddChild(arrow);

            arrow.GlobalPosition = GlobalPosition;
            arrow.Rotation = currentAngle;
            arrow.Damage = finalDamage;
        }
    }
    private void OnStaffTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null || StaffProjectileScene == null) return;

        int staffLevel = gm.GetUpgradeLevel("Staff");
        if (staffLevel <= 0) return;

        if (_waterstaffSound != null) _waterstaffSound.Play();

        int projectileCount = Math.Min(staffLevel, 4);

        float baseDmg = 7.0f + (staffLevel - 1) * 3.0f;
        int finalDamage = (int)(baseDmg * gm.GetStrengthMultiplier());

        float baseScale = 1.0f + (staffLevel - 1) * 0.15f;
        if (staffLevel >= 5)
        {
            baseScale += 0.5f;
        }

        float randomBaseAngle = (float)GD.RandRange(0, Mathf.Tau);

        float[] angleOffsets = { 0, Mathf.Pi / 2, Mathf.Pi, Mathf.Pi * 1.5f };

        for (int i = 0; i < projectileCount; i++)
        {
            var projectile = StaffProjectileScene.Instantiate<WaterProjectile>();
            GetParent().AddChild(projectile);

            projectile.GlobalPosition = GlobalPosition;
            projectile.Rotation = randomBaseAngle + angleOffsets[i];
            projectile.Damage = finalDamage;

            projectile.Scale = new Vector2(baseScale, baseScale);
        }
    }
    private void OnFireTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null || FireProjectileScene == null) return;

        int fireLevel = gm.GetUpgradeLevel("FireStaff");
        if (fireLevel <= 0) return;

        if (_firestaffSound != null) _firestaffSound.Play();

        int damage = (int)((5.0f + (fireLevel - 1) * 2.0f) * gm.GetStrengthMultiplier());

        float lifespan = 1.0f + (fireLevel - 1) * 0.5f;

        float scale = 1.0f + (fireLevel - 1) * 0.2f;

        var projectile = FireProjectileScene.Instantiate<FireProjectile>();
        GetParent().AddChild(projectile);

        projectile.GlobalPosition = GlobalPosition;
        projectile.Rotation = (float)GD.RandRange(0, Mathf.Tau);

        projectile.ZoneDamage = damage;
        projectile.ZoneLifespan = lifespan;
        projectile.ZoneScale = new Vector2(scale, scale);
    }
    public void UpdateAuraStatus()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null) return;

        int auraLevel = gm.GetUpgradeLevel("Aura");

        var aura = GetNodeOrNull<Aura>("Aura");

        if (aura != null && auraLevel > 0)
        {
            aura.Visible = true;
            aura.Monitoring = true;

            float baseDamage = 5.0f + (auraLevel - 1) * 2.0f;
            float strengthMult = gm.GetStrengthMultiplier();
            aura.Damage = (int)(baseDamage * strengthMult);

            float scaleValue = 1.0f + (auraLevel - 1) * 0.15f;
            aura.Scale = new Vector2(scaleValue, scaleValue);

            GD.Print($"[Аура] Оновлено! Рівень: {auraLevel}, Шкода: {aura.Damage}, Розмір: {scaleValue * 100}%");
        }
    }
}