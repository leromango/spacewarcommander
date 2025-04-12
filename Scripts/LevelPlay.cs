using Godot;
using Godot.Collections;
using System;

public enum EEnemyType
{
    NORMAL,
    HEAVY,
    FAST,
}

public enum EDifficulty
{
    EASY,
    NORMAL,
    HARD,
}

public partial class LevelPlay : Node3D
{
    [Export] AudioStream battle_music;
    [Export] bool bShouldPlayMusic = true;
    [Export] float BattleMusicVolumeDB = 5f;
    Player player;
    //[ExportCategory("Enemy Spawns")]
    [Export] float FogChangeTimeMax = 15;
    [Export] float FogDensity = 0.01f;
    [Export] PackedScene NormalEnemyScene;
    [Export] PackedScene FastEnemyScene;
    [Export] PackedScene HeavyEnemyScene;
    [Export] PackedScene ExplosionVFXScene;
    [Export] private int MaxEnemiesInLevel = 5;
    [Export] private int CurrentEnemiesNum = 0;
    [Export] private int EasyScore = 15;
    [Export] private int MediumScore = 30;
    private EDifficulty CurrentDifficulty = EDifficulty.EASY;
    Timer EnemySpawnHandlerTimer;
    Timer FogHandlerTimer;
    DirectionalLight3D LevelLight;
    FogVolume FogInLevel;
    WorldEnvironment env;

    public static LevelPlay Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        if (bShouldPlayMusic)
        {
            AudioStreamPlayer audioPlayer = new AudioStreamPlayer();
            audioPlayer.Stream = battle_music;
            audioPlayer.VolumeDb = BattleMusicVolumeDB;
            AddChild(audioPlayer);
            audioPlayer.Play();
        }
        Instance = this;
        EnemySpawnHandlerTimer = GetNode<Timer>("EnemySpawnHandlerTimer");
        FogHandlerTimer = GetNode<Timer>("FogHandlerTimer");
        player = (Player) GetNode("Player");
        LevelLight = GetNode<DirectionalLight3D>("DirectionalLight3D");
        FogInLevel = GetNode<FogVolume>("FogVolume");
        env = GetNode<WorldEnvironment>("WorldEnvironment");
    }

    public int GetCurrentNumOfEnemies()
    {
        return CurrentEnemiesNum;
    }

    private void _on_enemy_spawn_handler_timer_timeout()
    {
        SpawnEnemy();
    }

    private void _on_fog_handler_timer_timeout()
    {
        ChangeFogState();
        Random r = new Random();
        float random = Math.Clamp((r.NextSingle() + 0.2f) * 10, 0.1f, 1);
        FogHandlerTimer.WaitTime = random * FogChangeTimeMax;
    }

    private void ChangeFogState()
    {
        LevelLight.Visible = !LevelLight.Visible;
        env.Environment.VolumetricFogDensity = LevelLight.Visible ? 0 : FogDensity;
        FogInLevel.Visible = !LevelLight.Visible;
        env.Environment.FogEnabled = FogInLevel.Visible;
    }

    public void SpawnEnemy()
    {
        if (CurrentEnemiesNum >= MaxEnemiesInLevel) return;
        Random r = new Random();
        int EType = r.Next(0, 2);
        Node3D SpawnedEnemy = null;
        switch (EType)
        {
            case (int) EEnemyType.NORMAL:
                SpawnedEnemy = (Node3D)NormalEnemyScene.Instantiate();
                break;
            case (int) EEnemyType.FAST:
                SpawnedEnemy = (Node3D)FastEnemyScene.Instantiate();
                break;
            case (int)EEnemyType.HEAVY:
                SpawnedEnemy = (Node3D)HeavyEnemyScene.Instantiate();
                break;
            default: break;
        }
        SpawnedEnemy.Call("init_enemy", player.GlobalPosition);
        CurrentEnemiesNum++;
        AddChild(SpawnedEnemy);
    }

    public void enemy_death(Vector3 Location, int Score) {
        CpuParticles3D ExplosionParticles = (CpuParticles3D)ExplosionVFXScene.Instantiate();
        ExplosionParticles.GlobalPosition = Location;
        ExplosionParticles.Emitting = true;
        AddChild(ExplosionParticles);
        CurrentEnemiesNum--;
        player.AddScore(Score);
        if (player.GetCurrentScore() > MediumScore && CurrentDifficulty.Equals(EDifficulty.NORMAL))
        {
            CurrentDifficulty = EDifficulty.HARD;
            MaxEnemiesInLevel += 2;
            player.SetScoreColour(new Color(255, 0, 0));
        }
            
        else if (player.GetCurrentScore() > EasyScore && CurrentDifficulty.Equals(EDifficulty.EASY))
        {
            CurrentDifficulty = EDifficulty.NORMAL;
            MaxEnemiesInLevel++;
            player.SetScoreColour(new Color(255, 255, 0));
        }
    }
}
