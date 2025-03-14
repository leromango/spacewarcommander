using Godot;
using System;

public partial class Bullet : StaticBody3D
{
    [Export] private float speed = 10.0f;
    private Vector3 velocity;

    public override void _Ready()
    {
        velocity = Transform.Basis.Z * speed;
    }

    public override void _Process(double delta)
    {
        GlobalTransform = new Transform3D(GlobalTransform.Basis, GlobalTransform.Origin + velocity * (float)delta);
    }
}