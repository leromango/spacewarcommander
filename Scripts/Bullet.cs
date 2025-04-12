using Godot;
using System;

public partial class Bullet : StaticBody3D
{
    [Export] private float speed = 100.0f;
    private Area3D collisionArea;
    private float damage = 20;
    private Vector3 velocity;
    private Node3D ownerNode;
    [Export] PackedScene HitVFXScene;
    [Export] AudioStream hitSound;
    [Export] float hitAudioDB = -3f;

    public override void _Ready()
    {
        velocity = Transform.Basis.Z * speed;
        collisionArea = GetNode<Area3D>("BulletCollision");
    }

    public void initializeBullet(Node3D owner, Transform3D startingTransform, float damage)
    {
        this.damage = damage;
        this.ownerNode = owner;
        this.GlobalTransform = new Transform3D(startingTransform.Basis, startingTransform.Origin);
    }

    public override void _Process(double delta)
    {
        GlobalTransform = new Transform3D(GlobalTransform.Basis, GlobalTransform.Origin + velocity * (float)delta);
    }

    public void CreateHitVFX(Node3D nodeToAttachTo = null)
    {
        if (HitVFXScene == null) return;
        CpuParticles3D HitParticles = (CpuParticles3D)HitVFXScene.Instantiate();
        HitParticles.GlobalPosition = GlobalTransform.Origin;
        HitParticles.Emitting = true;
        // HitParticles.GlobalRotation = GlobalRotation;
        AudioStreamPlayer3D audioPlayer = new AudioStreamPlayer3D();
        audioPlayer.Finished += () => audioPlayer.QueueFree();
        audioPlayer.Stream = hitSound;
        audioPlayer.GlobalPosition = GlobalPosition;
        audioPlayer.GlobalRotation = GlobalRotation;
        audioPlayer.VolumeDb = hitAudioDB;

        if (nodeToAttachTo == null)
        {
            GetParentNode3D().AddChild(audioPlayer);
            GetParentNode3D().AddChild(HitParticles);
        }
            
        else
        {
            nodeToAttachTo.AddChild(HitParticles);
            nodeToAttachTo.AddChild(audioPlayer);
            HitParticles.GlobalPosition = GlobalTransform.Origin;
        }
           
        audioPlayer.Play();

    }

    public void _on_self_destruction_timer_timeout()
    {
        CreateHitVFX();
        QueueFree();
    }

    public void _on_bullet_collision_body_entered(Node3D body)
    {
        // When bullet collides with something
        if (body == null || ownerNode.IsQueuedForDeletion())
        {
            CreateHitVFX();
            QueueFree();
            return;
        }
        
        if (ownerNode != null) {
            if (body is StaticBody3D || ownerNode.Name.Equals(body.Name))
                return;
        }
        if (body is CharBase)
            ((CharBase)body).reduceHealth(damage, GlobalPosition);
        else
            body.Call("reduceHealth", damage);

        // GD.Print(ownerNode.Name + " COLLIDED WITH " + body.Name);
        CreateHitVFX(body);
        QueueFree();
    }
}