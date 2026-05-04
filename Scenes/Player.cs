using Godot;
using System;

/// <summary>
/// Головний клас гравця. Відповідає за переміщення, обробку характеристик (здоров'я, броня), 
/// отримання шкоди, регенерацію, а також керує таймерами та логікою всіх доступних видів зброї.
/// </summary>
public partial class Player : CharacterBody2D
{
    /// <summary>Базова швидкість пересування гравця.</summary>
    [Export] public float Speed { get; set; } = 150.0f;

    /// <summary>Шаблон сцени для ефекту удару мечем ближнього бою.</summary>
    [Export] public PackedScene SlashScene { get; set; }

    /// <summary>Максимальний запас здоров'я гравця.</summary>
    [Export] public int MaxHealth { get; set; } = 100;

    /// <summary>Додаткова шкода, яка додається до базових атак.</summary>
    [Export] public int BonusDamage { get; set; } = 0;

    /// <summary>Шаблон сцени стріли для лука.</summary>
    [Export] public PackedScene ArrowScene { get; set; }

    /// <summary>Шаблон сцени водяного снаряда для посоху води.</summary>
    [Export] public PackedScene StaffProjectileScene { get; set; }

    /// <summary>Шаблон сцени вогняного снаряда для вогняного посоху.</summary>
    [Export] public PackedScene FireProjectileScene { get; set; }

    private int _currentHealth = 100;
    private ProgressBar _hpBar;

    private AnimatedSprite2D _animatedSprite;

    /// <summary>Показник броні, який прямо зменшує отриману шкоду від ворогів.</summary>
    public int Armor { get; set; } = 0;
    private Timer _regenTimer;

    /// <summary>Публічне поле для зберігання поточного ліміту здоров'я гравця.</summary>
    public int MaxHp = 100;

    private AudioStreamPlayer _swordSound;
    private AudioStreamPlayer _damageSound;
    private AudioStreamPlayer _bowSound;
    private AudioStreamPlayer _firestaffSound;
    private AudioStreamPlayer _waterstaffSound;

    /// <summary>
    /// Викликається рушієм Godot при готовності вузла на сцені.
    /// Ініціалізує анімації, підключає інтерфейс смуги здоров'я, налаштовує аудіо 
    /// та динамічно створює таймери для автоматичних атак і регенерації.
    /// </summary>
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

    /// <summary>
    /// Обробляється кожен фізичний кадр гри.
    /// Відповідає за зчитування векторів введення для переміщення (WASD/стрілочки),
    /// керує рухом через <see cref="CharacterBody2D.MoveAndSlide"/> та оновлює спрайт анімації.
    /// </summary>
    /// <param name="delta">Час у секундах, що минув з попереднього фізичного кадру.</param>
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

    /// <summary>
    /// Завдає шкоди гравцю, враховуючи показник броні (мінімальна шкода завжди дорівнює 1).
    /// Оновлює смугу здоров'я, відтворює звук, робить візуальний спалах і перевіряє умови смерті.
    /// </summary>
    /// <param name="amount">Сила атаки ворога.</param>
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

    /// <summary>
    /// Відновлює здоров'я гравця. Не може перевищити максимальний ліміт (<see cref="MaxHealth"/>).
    /// Оновлює інтерфейс та створює зелений візуальний спалах лікування.
    /// </summary>
    /// <param name="amount">Кількість одиниць здоров'я для відновлення.</param>
    public void Heal(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth;

        if (_hpBar != null) _hpBar.Value = _currentHealth;

        Modulate = new Color(0, 1, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);
    }

    /// <summary>
    /// Обробляє загибель героя: знаходить в дереві сцени екран поразки (Game Over Menu) 
    /// та викликає його для завершення поточної ігрової сесії.
    /// </summary>
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
    }

    /// <summary>
    /// Обробник таймера базової атаки ближнього бою (Меч).
    /// Створює ефект удару, розраховує підсумкову шкоду та розмір зони ураження 
    /// на основі поточного рівня поліпшення та глобального множника сили.
    /// </summary>
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

    /// <summary>
    /// Обробник таймера пасивної регенерації.
    /// Перевіряє наявність поліпшення "Regen" у <see cref="GameManager"/> і лікує гравця відповідно до його рівня.
    /// </summary>
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

    /// <summary>
    /// Динамічно зменшує затримку між атаками мечем (AttackTimer) 
    /// на основі рівня поліпшення швидкості атаки (Жага Крові).
    /// </summary>
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

    /// <summary>
    /// Обробник таймера атаки луком.
    /// Випускає віяло стріл у випадковому напрямку, де кількість стріл і їхня шкода 
    /// залежать від рівня поліпшення "Bow" та глобального множника сили.
    /// </summary>
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

    /// <summary>
    /// Обробник таймера атаки посохом води.
    /// Стріляє пробивними снарядами на чотири сторони (або менше, залежно від рівня).
    /// На 5-му рівні значно збільшує масштаб і зону ураження снарядів.
    /// </summary>
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

    /// <summary>
    /// Обробник таймера атаки вогняним посохом.
    /// Випускає вибуховий снаряд у випадковому напрямку, радіус, час життя та шкода якого
    /// масштабуються з підвищенням рівня поліпшення "FireStaff".
    /// </summary>
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

    /// <summary>
    /// Оновлює стан магічної аури навколо гравця (увімкнення/вимкнення, розмір та шкоду).
    /// Викликається глобальним менеджером при виборі відповідного поліпшення.
    /// </summary>
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