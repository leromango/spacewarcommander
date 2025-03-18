using Godot;
using System;

public partial class Bullet : StaticBody3D
{
    [Export] private float speed = 100.0f;
    private Area3D collisionArea;
    private float damage = 20;
    private Vector3 velocity;

    public override void _Ready()
    {
        velocity = Transform.Basis.Z * speed;
        collisionArea = GetNode<Area3D>("BulletCollision");
    }

    public void setDamage(float damage)
    {
        this.damage = damage;
    }

    public override void _Process(double delta)
    {
        GlobalTransform = new Transform3D(GlobalTransform.Basis, GlobalTransform.Origin + velocity * (float)delta);
    }

    public void _on_bullet_collision_body_entered(Node3D body)
    {
        // When bullet collides with something
    }
}