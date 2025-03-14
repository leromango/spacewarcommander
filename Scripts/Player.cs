using Godot;
using System;

public partial class Player : CharBase
{
	
	[Export] private float rotationSpeed = 0.01f;
	// private float currentRotation = 0;
	[Export] private PackedScene bulletScene;

	private Node3D BulletPoint;
	private SpotLight3D flashlight;
	private Timer flashlightTimer;
	private bool rotAxis = true;

	[Export]private int maxBullets = 10;
	private int currBullets = 10;

	private float distribution = 0.5f;
	
	private int flickerAmount = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		BulletPoint = GetNode<Node3D>("BulletPoint");
		flashlight = GetNode<SpotLight3D>("Flashlight");
		flashlightTimer = GetNode<Timer>("Timer");
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("TurnRight"))
			setRotation(-rotationSpeed);
		if (Input.IsActionPressed("TurnLeft"))
			setRotation(rotationSpeed);
	}
	
	
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("SwitchDirection"))
			rotAxis = !rotAxis;
		if (@event.IsActionPressed("Shoot"))
			Shoot();
		if (@event.IsActionPressed("Lights"))
			flashlightTimer.Start();
		if (@event.IsActionPressed("Reload"))
			Reload();
	}
	
	
	
	private void setRotation(float rot)
	{
		Vector3 currRot = this.GetRotation();
		if (rotAxis)
			SetRotation(currRot + new Vector3(0, rot, 0));
		else 
			SetRotation(currRot + new Vector3(0, 0, rot));
	}
	
	protected override void Shoot()
	{
		base.Shoot();
		currBullets--;
		if (currBullets <= 0)
		{
			Eject();
			return;
		}
		if (bulletScene != null && BulletPoint != null)
		{
			Node3D bulletInstance = (Node3D)bulletScene.Instantiate();
			bulletInstance.GlobalTransform = BulletPoint.GlobalTransform;
			GetParent().AddChild(bulletInstance);
		}
	}
	
	private void ComputeDistribution()
	{
		Regenerate(1-distribution);
		SetDamage(1-distribution);
	}
	
	private void Regenerate(float regen)
	{
		CurrHealth += ((MaxHealth/3) * regen);
		if (CurrHealth > MaxHealth)
			CurrHealth = MaxHealth;
		else if (CurrHealth < 0)
			CurrHealth = 0;
	}

	private void SetDamage(float damage)
	{
		CurrDamage = (float)(MaxDamage * damage+0.1);
	}
	
	private void Eject()
	{
		
	}

	private void Reload()
	{
		currBullets = maxBullets;
	}
	
	private void _on_timer_timeout()
	{
		if (flickerAmount >= 4)
		{
			flashlightTimer.SetWaitTime(0.1);
			flashlight.Visible = !flashlight.Visible;
			flickerAmount = 0;
			flashlightTimer.Stop();
			return;
		}
		flashlight.Visible = !flashlight.Visible;
		flashlightTimer.SetWaitTime(flashlightTimer.GetWaitTime() + 0.05);
		flickerAmount++;
	}
}
