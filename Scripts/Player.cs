using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Player : CharBase
{
    [Export] private float _rotationSpeed = 0.01f;
    [Export] private PackedScene _bulletScene;
    [Export] private int _maxBullets = 10;
    private int _currBullets = 10;
    
    private Node3D _bulletPoint;
    private SpotLight3D _flashlight;
    private Timer _flashlightTimer;

    private MeshInstance3D _reloadBlock;
    private Transform3D _defaultReloadBlockTransform;
    private Transform3D _reloadBlockTransform;
    
    private Node3D _ammoContainer;
    private List<MeshInstance3D> _bulletSegments = new List<MeshInstance3D>();

    private bool _rotAxis = true;
    private float _distribution = 0.5f;
    private int _flickerAmount;

    private Material _matAmmoOn;
    private Material _matAmmoOff;

    private MeshInstance3D _damageIndicator;
    private MeshInstance3D _healthIndicator;
    
    private Vector3 _defaultDamageScale;
    private Vector3 _defaultHealthScale;

    private int _healthSegmentsCount = 10;
    private Node3D _healthContainer;
    private List<MeshInstance3D> _healthSegments = new List<MeshInstance3D>();
    private Material _matHealthOn;
    private Material _matHealthOff;

    public enum EShotDirectionIndication
    {
        FORWARD,
        LEFT,
        RIGHT,
        BACK,
        ALL,
    }

    public override void reduceHealth(float amount, Vector3 hitLocation)
    {
        base.reduceHealth(amount, hitLocation);
        EShotDirectionIndication direction = GetShotDirectionIndicator(hitLocation);
        GD.Print("DIRECTION SHOT: " + direction.ToString());
    }

    public override void _Ready()
    {
        base._Ready();

        _bulletPoint = GetNode<Node3D>("BulletPoint");
        _flashlight = GetNode<SpotLight3D>("Flashlight");
        _flashlightTimer = GetNode<Timer>("Timer");
        
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
        // _matHealthOff = GD.Load<Material>("res://Materials/Mat_Health_OFF.tres");
        
        // _matHealthOn = GD.Load<Material>("res://Materials/Mat_Ammo_ON.tres");
        _matHealthOff = GD.Load<Material>("res://Materials/Mat_Ammo_OFF.tres");
        

        CreateAmmoSegments();
        UpdateAmmoIndicator();
        ComputeDistribution();

        CreateHealthSegments();
        UpdateHealthSegments();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("TurnRight"))
            SetRotation(-_rotationSpeed);
        if (Input.IsActionPressed("TurnLeft"))
            SetRotation(_rotationSpeed);
        
        RegenerateHealth(delta);
        UpdateHealthSegments();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("SwitchDirection"))
            _rotAxis = !_rotAxis;
        if (@event.IsActionPressed("Shoot"))
            Shoot();
        if (@event.IsActionPressed("Lights"))
            _flashlightTimer.Start();
        if (@event.IsActionPressed("Reload"))
            Reload();

        if (@event.IsActionPressed("ResourceManagmentUp"))
        {
            _distribution = Mathf.Clamp(_distribution + 0.1f, 0f, 1f);
            ComputeDistribution();
        }
        if (@event.IsActionPressed("ResourceManagmentDown"))
        {
            _distribution = Mathf.Clamp(_distribution - 0.1f, 0f, 1f);
            ComputeDistribution();
        }
        
        if (@event.IsActionPressed("SelfDamage")) ApplyDamage(10f);
    }
    
    private void SetRotation(float rot)
    {
        Vector3 currRot = GetRotation();
        if (_rotAxis)
            SetRotation(currRot + new Vector3(0, rot, 0));
        else 
            SetRotation(currRot + new Vector3(0, 0, rot));
    }
    
    protected override void Shoot()
    {
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
        }
    }
    
    private void Reload()
    {
        _currBullets = _maxBullets;
        _reloadBlock.SetTransform(_defaultReloadBlockTransform);
        UpdateAmmoIndicator();
    }

    private void UpdateAmmoIndicator()
    {
        for (int i = 0; i < _bulletSegments.Count; i++)
        {
            if (i < _currBullets)
                _bulletSegments[i].MaterialOverride = _matAmmoOn;
            else
                _bulletSegments[i].MaterialOverride = _matAmmoOff;
        }
    }
    
    private void Eject()
    {
        _reloadBlock.SetTransform(_reloadBlockTransform);
    }
    
    private void _on_timer_timeout()
    {
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
    
    private void CreateAmmoSegments()
    {
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
            if (i < activeSegments)
                _healthSegments[i].MaterialOverride = _matHealthOn;
            else
                _healthSegments[i].MaterialOverride = _matHealthOff;
        }
    }
    
    private void ApplyDamage(float damage)
    {
        CurrHealth = Mathf.Max(CurrHealth - damage, 0);
        UpdateHealthSegments();
    }

    public EShotDirectionIndication GetShotDirectionIndicator(Vector3 hitLocation) {
        EShotDirectionIndication indicatorResult = EShotDirectionIndication.ALL;  // By default for when it is diagonal
        Vector3 hitLocationLocal = (hitLocation - GlobalPosition).Rotated(new Vector3(1,0,0), Rotation.X)
            .Rotated(new Vector3(0, 1, 0), Rotation.Y)
            .Rotated(new Vector3(0, 0, 1), Rotation.Z);
        const float threshold = 2;
        float distanceZ = Math.Abs(hitLocationLocal.Z);
        float distanceY = Math.Abs(hitLocationLocal.Y);
        float distanceX = Math.Abs(hitLocationLocal.X);
        // Up and down is basically forward and back
        if (distanceX >= threshold && distanceX > distanceY && distanceX > distanceZ) {
            if (hitLocationLocal.X > 0)
                indicatorResult = EShotDirectionIndication.FORWARD;
            else if (hitLocationLocal.X < 0)
                indicatorResult = EShotDirectionIndication.BACK;
        }
        else if (distanceY >= threshold && distanceY > distanceX && distanceY > distanceZ)
        {
            if (hitLocationLocal.Y > 0)
                indicatorResult = EShotDirectionIndication.FORWARD;
            else if (hitLocationLocal.Y < 0)
                indicatorResult = EShotDirectionIndication.BACK;
        }
        else if (distanceZ >= threshold && distanceZ > distanceX && distanceZ > distanceY) {
            if (hitLocationLocal.Z > 0)
                indicatorResult = EShotDirectionIndication.LEFT;
            else if (hitLocationLocal.Z < 0)
                indicatorResult = EShotDirectionIndication.RIGHT;
        }

        return indicatorResult;
    }
}