using Godot;
using System;

public partial class CharBase : CharacterBody3D
{
	[Export] protected float MaxHealth; 
	protected float CurrHealth;
	[Export] protected float MaxDamage;
	protected float CurrDamage;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrHealth = MaxHealth;
		CurrDamage = MaxDamage;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	protected virtual void Shoot() { }
	
	protected virtual void Die() { }
	
	public float getHealth() { return CurrHealth; }
	public float getDamage() { return CurrDamage; }
	
}
