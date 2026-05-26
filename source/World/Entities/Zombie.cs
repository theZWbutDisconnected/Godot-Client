using System;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render.Model;
using TestClient.Source.Render.Model.impl;
using TestClient.Source.Utility;

namespace TestClient.Source.World.Entities;

public class Zombie : ServerLiving
{
    public Zombie(Level level) : base(level)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        DataWatcher.AddObject(12, (byte)0);
        DataWatcher.AddObject(13, (byte)0);
        DataWatcher.AddObject(14, (byte)0);
    }

    public override bool IsChild()
    {
        return DataWatcher.GetWatched(12) == 1;
    }

    protected override ModelRenderer GetModelRenderer()
    {
        return Renderer ??= new ModelRenderer(new ZombieModel(), "res://assets/entity/zombie.png", Game.Singleton, 64, 64);
    }
}