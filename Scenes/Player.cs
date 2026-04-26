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

    private int _currentHealth;
    private ProgressBar _hpBar;

    private AnimatedSprite2D _animatedSprite;

    public int Armor { get; set; } = 0;
    private Timer _regenTimer;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        GetNode<Timer>("AttackTimer").Timeout += OnAttackTimerTimeout;

        AddToGroup("Player");

        _currentHealth = MaxHealth;

        _hpBar = GetTree().Root.GetNodeOrNull<ProgressBar>("Main/UI/HpBar");
        if (_hpBar != null)
        {
            _hpBar.MaxValue = MaxHealth;
            _hpBar.Value = _currentHealth;
        }

        _animatedSprite.Play("idle");

        // Створюємо таймер регенерації програмно
        _regenTimer = new Timer();
        _regenTimer.WaitTime = 5.0f; // Кожні 5 секунд
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
        // Броня поглинає шкоду, але ворог завжди наносить мінімум 1 одиницю
        int finalDamage = Math.Max(1, amount - Armor);

        _currentHealth -= finalDamage;

        if (_hpBar != null) _hpBar.Value = _currentHealth;

        Modulate = new Color(1, 0, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);

        if (_currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        if (_currentHealth > MaxHealth) _currentHealth = MaxHealth; // Не більше максимуму

        if (_hpBar != null) _hpBar.Value = _currentHealth;

        // зелений спалах
        Modulate = new Color(0, 1, 0);
        GetTree().CreateTimer(0.1f).Timeout += () => Modulate = new Color(1, 1, 1);
    }

    private void Die()
    {
        GD.Print("ГРАВЕЦЬ ЗАГИНУВ! ГРУ ЗАКІНЧЕНО!");
        GetTree().ReloadCurrentScene();
    }

    private void OnAttackTimerTimeout()
    {
        if (SlashScene == null) return;

        var gm = GetTree().Root.GetNode<GameManager>("Main/GameManager");
        var slash = SlashScene.Instantiate<SlashEffect>();

        // 1. РОЗРАХУНОК ШКОДИ
        // Беремо базову шкоду меча + бонуси за рівень меча, і в самому кінці множимо на відсоток Сили.
        float baseSwordDamage = 10.0f;
        int swordLevel = gm.GetUpgradeLevel("Sword");
        float swordLevelBonus = (swordLevel - 1) * 5.0f;

        float strengthMult = gm.GetStrengthMultiplier();

        // Підсумкова формула
        slash.Damage = (int)((baseSwordDamage + swordLevelBonus) * strengthMult);

        // 2. ФІЗИЧНЕ ТА ВІЗУАЛЬНЕ МАСШТАБУВАННЯ
        // 10% росту за кожен рівень після першого
        float radiusScale = 1.0f + (swordLevel - 1) * 0.1f;
        slash.Scale = new Vector2(radiusScale, radiusScale);

        // позиція, віддзеркалення
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
                Heal(regenLevel); // Лікуємо 1 ХП за кожен рівень регенерації
            }
        }
    }

    public void UpdateAttackSpeed()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm != null)
        {
            int cdLevel = gm.GetUpgradeLevel("Cooldown");

            // Базовий час 1.2 сек. Зменшуємо на 0.2 за рівень
            float newWaitTime = 1.2f - (cdLevel * 0.2f);

            // Ставимо жорсткий ліміт, щоб зброя не стріляла швидше ніж 5 разів на секунду
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

        // Якщо лук ще не куплено — не стріляємо
        if (bowLevel <= 0 || ArrowScene == null) return;

        // 1. Розрахунок кількості стріл (1 стріла на 1 рівні, 2 на 2-му і т.д.)
        int arrowCount = bowLevel;

        // 2. Розрахунок шкоди (Базова 15 + 5% за кожен рівень) * Множник Сили
        float baseArrowDamage = 15.0f;
        float levelMultiplier = 1.0f + (bowLevel * 0.05f);
        float strengthMult = gm.GetStrengthMultiplier();
        int finalDamage = (int)(baseArrowDamage * levelMultiplier * strengthMult);

        // 3. Випадковий центральний напрямок
        float randomAngle = (float)GD.RandRange(0, Mathf.Tau);

        // 4. Спавн стріл віялом
        for (int i = 0; i < arrowCount; i++)
        {
            // Формула розрахунку зсуву для кожної стріли (15 градусів = 0.2618 радіан)
            float angleOffset = (i - (arrowCount - 1) / 2.0f) * Mathf.DegToRad(15);
            float currentAngle = randomAngle + angleOffset;

            var arrow = ArrowScene.Instantiate<Arrow>();
            GetParent().AddChild(arrow);

            arrow.GlobalPosition = GlobalPosition;
            arrow.Rotation = currentAngle; // Стріла полетить туди, куди дивиться
            arrow.Damage = finalDamage;
        }
    }
    private void OnStaffTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null || StaffProjectileScene == null) return;

        int staffLevel = gm.GetUpgradeLevel("Staff");
        if (staffLevel <= 0) return;

        // 1. КІЛЬКІСТЬ СНАРЯДІВ (ліміт 4)
        int projectileCount = Math.Min(staffLevel, 4);

        // 2. РОЗРАХУНОК ШКОДИ
        float baseDmg = 7.0f + (staffLevel - 1) * 3.0f;
        int finalDamage = (int)(baseDmg * gm.GetStrengthMultiplier());

        // 3. РОЗРАХУНОК РОЗМІРУ (Спеціальний бонус на 5 рівні)
        float baseScale = 1.0f + (staffLevel - 1) * 0.15f;
        if (staffLevel >= 5)
        {
            baseScale += 0.5f; // Додатковий великий бонус до радіусу на макс. рівні
        }

        // 4. ГЕНЕРУЄМО ВИПАДКОВИЙ БАЗОВИЙ КУТ
        float randomBaseAngle = (float)GD.RandRange(0, Mathf.Tau);

        // 5. ВІДСТАНЬ МІЖ СНАРЯДАМИ (90 градусів)
        float[] angleOffsets = { 0, Mathf.Pi / 2, Mathf.Pi, Mathf.Pi * 1.5f };

        // Цикл спрацює стільки разів, скільки у нас снарядів
        for (int i = 0; i < projectileCount; i++)
        {
            var projectile = StaffProjectileScene.Instantiate<WaterProjectile>();
            GetParent().AddChild(projectile);

            projectile.GlobalPosition = GlobalPosition;
            projectile.Rotation = randomBaseAngle + angleOffsets[i];
            projectile.Damage = finalDamage;

            // Застосовуємо розрахований масштаб
            projectile.Scale = new Vector2(baseScale, baseScale);
        }
    }
    private void OnFireTimerTimeout()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null || FireProjectileScene == null) return;

        int fireLevel = gm.GetUpgradeLevel("FireStaff");
        if (fireLevel <= 0) return;

        // РОЗРАХУНКИ ПОЛІПШЕНЬ:
        // Базова шкода 5 + 2 за рівень
        int damage = (int)((5.0f + (fireLevel - 1) * 2.0f) * gm.GetStrengthMultiplier());

        // Час життя: 1 секунда + 0.5 за кожен рівень після першого
        float lifespan = 1.0f + (fireLevel - 1) * 0.5f;

        // Масштаб зони: збільшується на 20% за кожен рівень
        float scale = 1.0f + (fireLevel - 1) * 0.2f;

        // Стріляємо в рандомному напрямку
        var projectile = FireProjectileScene.Instantiate<FireProjectile>();
        GetParent().AddChild(projectile);

        projectile.GlobalPosition = GlobalPosition;
        projectile.Rotation = (float)GD.RandRange(0, Mathf.Tau);

        // Передаємо розраховані дані снаряду (а він потім передасть їх зоні)
        projectile.ZoneDamage = damage;
        projectile.ZoneLifespan = lifespan;
        projectile.ZoneScale = new Vector2(scale, scale);
    }
    public void UpdateAuraStatus()
    {
        var gm = GetTree().Root.GetNodeOrNull<GameManager>("Main/GameManager");
        if (gm == null) return;

        int auraLevel = gm.GetUpgradeLevel("Aura");

        // Шукаємо нашу ауру
        var aura = GetNodeOrNull<Aura>("Aura");

        if (aura != null && auraLevel > 0)
        {
            // 1. Вмикаємо візуал та фізику
            aura.Visible = true;
            aura.Monitoring = true;

            // 2. Рахуємо шкоду: Базова (5) + (2 за кожен рівень) * Множник Сили
            float baseDamage = 5.0f + (auraLevel - 1) * 2.0f;
            float strengthMult = gm.GetStrengthMultiplier();
            aura.Damage = (int)(baseDamage * strengthMult);

            // 3. Рахуємо розмір: Збільшуємо на 15% за рівень
            float scaleValue = 1.0f + (auraLevel - 1) * 0.15f;
            aura.Scale = new Vector2(scaleValue, scaleValue);

            GD.Print($"[Аура] Оновлено! Рівень: {auraLevel}, Шкода: {aura.Damage}, Розмір: {scaleValue * 100}%");
        }
    }
}