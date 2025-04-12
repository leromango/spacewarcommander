using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharBase
{
    [Export] float  shootAudioDB = 5f;
    [Export] float  deathAudioDB = 5f;
    [Export] private AudioStream shootSound;
    [Export] private AudioStream deathSound;
    private Timer flashlightAvailableDelayTimer;
    // Sensivity
    [Export] private float _rotationSpeed = 0.1f; // Rotation speed for the player
    // --- Basic shooting and energy properties ---
    [Export] private bool CanUseKeyboardToPlay = false;
    [Export] private PackedScene _bulletScene;
    [Export] private int _maxBullets = 10;
    private int _currBullets = 10;

    private bool bCanFlashlightTurnOn = true;
    private float _flashlightEnergyLeft = 0;
    [Export] private float _maximumFlashlightEnergy = 20f;
    [Export] private float _flashlightEnergyRecoveryRate = 0.2f;
    [Export] private float _flashlightEnergyDrainRate = 0.4f;

    private Node3D _bulletPoint;
    private SpotLight3D _flashlight;
    private Timer _flashlightTimer;
    private Timer _LowHealthBuzzerTimer;
    [Export] private float _buzzerTimerMin = 0.1f;
    [Export] private float _buzzerTimerMax = 1f;
    [Export] private float _buzzerHealthThreshold = 50f;
    private Timer _SendMessageTimer;

    // --- Reload and ammo indicators ---
    private MeshInstance3D _reloadBlock;
    private Transform3D _defaultReloadBlockTransform;
    private Transform3D _reloadBlockTransform;
    
    private Node3D _ammoContainer;
    private List<MeshInstance3D> _bulletSegments = new List<MeshInstance3D>();
    // Materials used for ammo indication.
    private Material _matAmmoOn;
    private Material _matAmmoOff;

    // --- Scoring and UI ---
    private int CurrentScore = 0;
    private StandardMaterial3D FlashlightEnergyIndicatorMat;
    private Label3D ScoreAmountLabel;

    // --- Health and damage UI ---
    private Vector3 _defaultDamageScale;
    private Vector3 _defaultHealthScale;
    private MeshInstance3D _damageIndicator;
    private MeshInstance3D _healthIndicator;
    private int _healthSegmentsCount = 10;
    private Node3D _healthContainer;
    private List<MeshInstance3D> _healthSegments = new List<MeshInstance3D>();
    private Material _matHealthOn;
    private Material _matHealthOff;
    private float _distribution;  // Resource distribution value

    // --- Communication node ---
    private Node _serCommNode;

    // --- Reload signal flag (to avoid sending multiple "r" commands), DIDNT HELP WITH FUCKING SERVO ---
    private bool _reloadSignalSent = false;

    // --- Shot direction indicator enum ---
    public enum EShotDirectionIndication
    {
        FORWARD,
        LEFT,
        RIGHT,
        BACK,
        ALL,
    }

    private float GetBuzzerBuzzTime()
    {
        return _buzzerTimerMin + ((_buzzerTimerMax - _buzzerTimerMin) / (_buzzerHealthThreshold - 10)) * (CurrHealth - 10);
    }

    public int GetCurrentScore() { return CurrentScore; }

    private void HandleFlashlightEnergy(float delta)
    {
        _flashlightEnergyLeft = Math.Clamp(_flashlight.Visible ? _flashlightEnergyLeft - (_flashlightEnergyDrainRate * delta)
                                                             : _flashlightEnergyLeft + (_flashlightEnergyRecoveryRate * delta),
                                          0f, _maximumFlashlightEnergy);
        if (_flashlightEnergyLeft <= 2 && bCanFlashlightTurnOn) {
            bCanFlashlightTurnOn = false;
            flashlightAvailableDelayTimer.Start();
            _flashlight.Visible = false;
        }
        FlashlightEnergyIndicatorMat.EmissionEnergyMultiplier = (3.85f / _maximumFlashlightEnergy) * _flashlightEnergyLeft;
    }

    public void SetScoreColour(Color color)
    {
        ScoreAmountLabel.Modulate = color;
        ScoreAmountLabel.OutlineModulate = color;
    }

    public void toggleFlashlight(bool value) {
        if (CurrHealth <= 0 || _flashlightEnergyLeft <= 0 || !bCanFlashlightTurnOn)
            return;
        _flashlight.Visible = value;
    }
    
    public void _on_flashlight_available_delay_timer_timeout() {
        bCanFlashlightTurnOn = true;
    }

    public override void reduceHealth(float amount, Vector3 hitLocation)
    {
        base.reduceHealth(amount, hitLocation);
        if (CurrHealth <= _buzzerHealthThreshold)
        {
            SendBuzzerBuzz();
            _LowHealthBuzzerTimer.Start(GetBuzzerBuzzTime());
        }
        SendLightIndicator(GetShotDirectionIndicator(hitLocation));
    }

    protected override void Die()
    {
        base.Die();
        AudioStreamPlayer3D audioPlayer = new AudioStreamPlayer3D();
        audioPlayer.Finished += () => audioPlayer.QueueFree();
        audioPlayer.Stream = deathSound;
        audioPlayer.VolumeDb = deathAudioDB;
        audioPlayer.GlobalPosition = GlobalTransform.Origin;
        GetParentNode3D().AddChild(audioPlayer);
        audioPlayer.Play();
    }

    public override void _Ready()
    {
        base._Ready();
        flashlightAvailableDelayTimer = GetNode<Timer>("FlashlightAvailableDelayTimer");
        _flashlightEnergyLeft = _maximumFlashlightEnergy;
        FlashlightEnergyIndicatorMat = (StandardMaterial3D)GetNode<MeshInstance3D>("Meshes/FlashlightEnergyIndicator").GetActiveMaterial(0);
        ScoreAmountLabel = GetNode<Label3D>("Meshes/ScoreAmountLabel");

        _serCommNode = GetNode("SerComm");
        _bulletPoint = GetNode<Node3D>("BulletPoint");
        _flashlight = GetNode<SpotLight3D>("Flashlight");
        _flashlightTimer = GetNode<Timer>("Timer");
        _LowHealthBuzzerTimer = GetNode<Timer>("LowHealthBuzzerTimer");
        _SendMessageTimer = GetNode<Timer>("SerComm/SendMessageTimer");

        _reloadBlock = GetNode<MeshInstance3D>("Meshes/Reload");
        _defaultReloadBlockTransform = _reloadBlock.GetTransform();
        _reloadBlockTransform = new Transform3D(
            new Basis(Vector3.Forward, Mathf.DegToRad(9.9f)),
            new Vector3(-8.532f, 2.488f, 2.853f)
        );
        
        _matAmmoOn = GD.Load<Material>("res://Materials/Mat_Ammo_ON.tres");
        _matAmmoOff = GD.Load<Material>("res://Materials/Mat_Ammo_OFF.tres");
        
        _damageIndicator = GetNode<MeshInstance3D>("Meshes/Damage");
        _healthIndicator = GetNode<MeshInstance3D>("Meshes/HealthRegen");

        _defaultDamageScale = _damageIndicator.Scale;
        _defaultHealthScale = _healthIndicator.Scale;
        
        _matHealthOn = GD.Load<Material>("res://Materials/Mat_Health_ON.tres");
        _matHealthOff = GD.Load<Material>("res://Materials/Mat_Ammo_OFF.tres");

        CreateAmmoSegments();
        UpdateAmmoIndicator();
        ComputeDistribution();

        CreateHealthSegments();
        UpdateHealthSegments();
    }

    // --- Slider inputs for rotation ---
    [Export] public int SliderXValue = 512; // Horizontal slider value (0 to 1024)
    [Export] public int SliderYValue = 512; // Vertical slider value (0 to 1024)
    [Export] private float _maxRotationOffset = Mathf.DegToRad(30); // Maximum rotation offset (radians)

    /// <summary>
    /// Maps a slider value to a rotation delta.
    /// </summary>
    private float MapSliderToRotation(int sliderValue, int lowThreshold, int highThreshold, float maxRotation)
    {
        if (sliderValue < lowThreshold)
        {
            // Map [0, lowThreshold] to [-maxRotation, 0]
            return Mathf.Lerp(-maxRotation, 0, sliderValue / (float)lowThreshold);
        }
        else if (sliderValue > highThreshold)
        {
            // Map [highThreshold, 1024] to [0, maxRotation]
            return Mathf.Lerp(0, maxRotation, (sliderValue - highThreshold) / (1024f - highThreshold));
        }
        return 0f;
    }

    /// <summary>
    /// Adds incremental rotation based on the slider values.
    /// Horizontal (yaw) is controlled by SliderXValue and vertical (pitch) by SliderYValue.
    /// Both axes are inverted so that:
    ///   - For horizontal: a 0 value causes left rotation (decreasing yaw).
    ///   - For vertical: a 0 value causes upward rotation (decreasing pitch).
    /// </summary>
    public void AddRotationFromSliders()
    {
        // Invert the X slider: multiply by -1 to flip the mapping.
        float yawDelta = -MapSliderToRotation(SliderXValue, 490, 600, _maxRotationOffset) * _rotationSpeed;
        // Invert the Y slider to make 0 generate a negative pitch (look up)
        float pitchDelta = -MapSliderToRotation(SliderYValue, 490, 600, _maxRotationOffset) * _rotationSpeed;

        Rotation = new Vector3(Rotation.X, Rotation.Y + yawDelta, Rotation.Z - pitchDelta);
    }
    
    public void UpdateRotation()
    {
        AddRotationFromSliders();
    }
    
    public override void _Process(double delta)
    {
        if (CurrHealth <= 0)
            return;
        RegenerateHealth(delta);
        UpdateHealthSegments();
        HandleFlashlightEnergy((float)delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (CurrHealth <= 0 || !CanUseKeyboardToPlay)
            return;
        if (Input.IsActionPressed("Move"))
            AddRotationFromSliders();
    }

    protected override void Shoot()
    {
        if (CurrHealth <= 0 || _currBullets <= 0)
            return;
        base.Shoot();

        _currBullets--;
        UpdateAmmoIndicator();

        if (_currBullets <= 0)
        {
            Eject();
            return;
        }
        if (_bulletScene != null && _bulletPoint != null)
        {
            Bullet bulletInstance = (Bullet)_bulletScene.Instantiate();
            bulletInstance.initializeBullet(this, _bulletPoint.GlobalTransform, CurrDamage);
            GetParent().AddChild(bulletInstance);
            AudioStreamPlayer3D audioPlayer = new AudioStreamPlayer3D();
            audioPlayer.Finished += () => audioPlayer.QueueFree();
            audioPlayer.Stream = shootSound;
            audioPlayer.VolumeDb = shootAudioDB;
            audioPlayer.GlobalPosition = GlobalTransform.Origin;
            bulletInstance.AddChild(audioPlayer);
            audioPlayer.Play();
           
        }
    }

    /// <summary>
    /// Sets the bullet count back to full and resets the reload signal.
    /// </summary>
    private void Reload()
    {
        if (CurrHealth <= 0)
            return;
        _currBullets = _maxBullets;
        _reloadBlock.SetTransform(_defaultReloadBlockTransform);
        UpdateAmmoIndicator();
        _reloadSignalSent = false;
    }

    /// <summary>
    /// Updates the visual ammo indicator. This now runs even if there are 0 bullets.
    /// </summary>
    private void UpdateAmmoIndicator()
    {
        for (int i = 0; i < _bulletSegments.Count; i++)
        {
            _bulletSegments[i].MaterialOverride = (i < _currBullets) ? _matAmmoOn : _matAmmoOff;
        }
    }

    /// <summary>
    /// Called when the magazine is empty. Sends a reload prompt if one hasn’t already been sent.
    /// </summary>
    public void Eject()
    {
        _reloadBlock.SetTransform(_reloadBlockTransform);
        if (!_reloadSignalSent)
        {
            SendReloadPrompt();
            _reloadSignalSent = true;
        }
    }

    private int _flickerAmount;
    private void _on_timer_timeout()
    {
        if (CurrHealth <= 0 || _flashlightEnergyLeft <= 0)
            return;
        if (_flickerAmount >= 4)
        {
            _flashlightTimer.SetWaitTime(0.07);
            _flashlight.Visible = !_flashlight.Visible;
            _flickerAmount = 0;
            _flashlightTimer.Stop();
            return;
        }
        _flashlight.Visible = !_flashlight.Visible;
        _flashlightTimer.SetWaitTime(_flashlightTimer.GetWaitTime() + 0.02);
        _flickerAmount++;
    }

    private void _on_low_health_buzzer_timer_timeout()
    {
    }

    private void CreateAmmoSegments()
    {
        if (CurrHealth <= 0)
            return;
        _ammoContainer = new Node3D { Name = "AmmoContainer" };
        _reloadBlock.AddChild(_ammoContainer);
        _ammoContainer.Owner = this;

        float startZ = 0.385f;
        float endZ = -0.385f;
        float totalDistance = startZ - endZ;
        float step = totalDistance / (_maxBullets - 1);

        for (int i = 0; i < _maxBullets; i++)
        {
            var bulletBox = new MeshInstance3D
            {
                Name = $"BulletSegment{i}",
                Mesh = new BoxMesh()
            };

            bulletBox.Position = new Vector3(0.3f, 0f, startZ - (step * i));
            bulletBox.Scale = new Vector3(0.1f, 0.1f, totalDistance / _maxBullets * 0.8f);

            _ammoContainer.AddChild(bulletBox);
            _bulletSegments.Add(bulletBox);
        }
    }

    public void SetDistribution(float dist)
    {
        _distribution = dist;
        ComputeDistribution();
    }

    private void ComputeDistribution()
    {
        _healthIndicator.Scale = new Vector3(
            _defaultHealthScale.X,
            _defaultHealthScale.Y,
            _defaultHealthScale.Z * (1f - _distribution)
        );
        _damageIndicator.Scale = new Vector3(
            _defaultDamageScale.X,
            _defaultDamageScale.Y,
            _defaultDamageScale.Z * _distribution
        );
    }

    private void RegenerateHealth(double delta)
    {
        float regenRate = (1f - _distribution) * 5f;
        CurrHealth = Mathf.Min(CurrHealth + regenRate * (float)delta, MaxHealth);
    }

    private void CreateHealthSegments()
    {
        Node3D meshes = GetNode<Node3D>("Meshes");
        _healthContainer = new Node3D { Name = "HealthContainer" };
        _healthContainer.SetPosition(new Vector3(-9.669f, 2.589f, 0.975f));
        meshes.AddChild(_healthContainer);
        _healthContainer.Owner = this;

        float startZ = 0.7f;
        float endZ = -0.7f;
        float totalDistance = startZ - endZ;
        float step = totalDistance / (_healthSegmentsCount - 1);

        for (int i = 0; i < _healthSegmentsCount; i++)
        {
            var segment = new MeshInstance3D
            {
                Name = $"HealthSegment{i}",
                Mesh = new BoxMesh()
            };

            segment.Position = new Vector3(0, 0, startZ - (step * i));
            segment.Scale = new Vector3(0.1f, 0.5f, 0.1f);
            segment.RotationDegrees = new Vector3(0, 0, 60);
            _healthContainer.AddChild(segment);
            _healthSegments.Add(segment);
        }
    }

    private void UpdateHealthSegments()
    {
        float ratio = CurrHealth / MaxHealth;
        int activeSegments = (int)Mathf.Round(ratio * _healthSegmentsCount);
        for (int i = 0; i < _healthSegments.Count; i++)
        {
            _healthSegments[i].MaterialOverride = (i < activeSegments) ? _matHealthOn : _matHealthOff;
        }
    }

    private void ApplyDamage(float damage)
    {
        if (CurrHealth <= 0)
            return;
        CurrHealth = Mathf.Max(CurrHealth - damage, 0);
        UpdateHealthSegments();
    }

    public EShotDirectionIndication GetShotDirectionIndicator(Vector3 hitLocation)
    {
        EShotDirectionIndication indicatorResult = EShotDirectionIndication.ALL;
        Vector3 hitLocationLocal = (hitLocation - GlobalPosition)
            .Rotated(new Vector3(1, 0, 0), Rotation.X)
            .Rotated(new Vector3(0, 1, 0), Rotation.Y)
            .Rotated(new Vector3(0, 0, 1), Rotation.Z);
        const float threshold = 2;
        float distanceZ = Math.Abs(hitLocationLocal.Z);
        float distanceY = Math.Abs(hitLocationLocal.Y);
        float distanceX = Math.Abs(hitLocationLocal.X);
        if (distanceX >= threshold && distanceX > distanceY && distanceX > distanceZ)
            indicatorResult = hitLocationLocal.X > 0 ? EShotDirectionIndication.FORWARD : EShotDirectionIndication.BACK;
        else if (distanceY >= threshold && distanceY > distanceX && distanceY > distanceZ)
            indicatorResult = hitLocationLocal.Y > 0 ? EShotDirectionIndication.FORWARD : EShotDirectionIndication.BACK;
        else if (distanceZ >= threshold && distanceZ > distanceX && distanceZ > distanceY)
            indicatorResult = hitLocationLocal.Z > 0 ? EShotDirectionIndication.LEFT : EShotDirectionIndication.RIGHT;
        else {
            if (distanceX >= threshold)
                indicatorResult = hitLocationLocal.X > 0 ? EShotDirectionIndication.FORWARD : EShotDirectionIndication.BACK;
            else if (distanceY >= threshold)
                indicatorResult = hitLocationLocal.Y > 0 ? EShotDirectionIndication.FORWARD : EShotDirectionIndication.BACK;
            else if (distanceZ >= threshold)
                indicatorResult = hitLocationLocal.Z > 0 ? EShotDirectionIndication.LEFT : EShotDirectionIndication.RIGHT;
        }
        return indicatorResult;
    }

    public void SendReloadPrompt()
    {
        _serCommNode.Call("send_reload_prompt");
    }

    public void SendBuzzerBuzz()
    {
        _serCommNode.Call("send_buzzer_buzz", GetBuzzerBuzzTime());
    }

    public void SendLightIndicator(EShotDirectionIndication direction)
    {
        _serCommNode.Call("send_light_indicator", ((int)direction));
    }

    public void _on_send_message_timer_timeout()
    {
        if (CurrHealth <= 0)
            return;
        _SendMessageTimer.Start();
    }

    public void AddScore(int ScoreToAdd)
    {
        CurrentScore += ScoreToAdd;
        ScoreAmountLabel.Text = CurrentScore.ToString();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ResetLevel"))
            GetTree().ReloadCurrentScene();
            //reload level 
        if (!CanUseKeyboardToPlay)
            return;
        if (@event.IsActionPressed("Shoot"))
            Shoot();
        if (@event.IsActionPressed("Lights"))
            _flashlightTimer.Start();
        if (@event.IsActionPressed("Reload"))
            Reload();
        // move/changeRotation inputs removed, using sliders now.
    }
}