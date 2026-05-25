using System;
using Godot;
using TestClient.Source.Render.Model;
using TestClient.Source.Render.Model.impl;

namespace TestClient.Source.World.Entities;

public class Zombie : Entity
{
    private readonly ZombieModel _model = new();
    private readonly ModelRenderer _renderer;
    private float _rot;
    private float _rotA;
    private float _speed;
    private float _timeOffs;

    public Zombie(Level level, double x, double y, double z) : base(level)
    {
        _rotA = (float)(new Random().NextDouble() + 1.0F) * 0.01F;
        SetPos(x, y, z);
        _timeOffs = (float)new Random().NextDouble() * 1239813.0F;
        _rot = (float)(new Random().NextDouble() * Math.PI * 2.0F);
        _speed = 1.0F;
        Game.Singleton.NewEntityNode(this, _renderer = new ModelRenderer(_model, "res://assets/entity/zombie.png", Game.Singleton, 64, 64));
    }

    public override void Tick()
    {
        base.Tick();
        if (PosY < -100.0F) Remove();

        _rot += _rotA;
        _rotA = (float)(_rotA * 0.99);
        _rotA = (float)(_rotA + (new Random().NextDouble() - new Random().NextDouble()) * new Random().NextDouble() *
            new Random().NextDouble() * 0.08F);
        RotY = Mathf.RadToDeg(_rot);
        if (OnGround && new Random().NextDouble() < 0.08) YDelta = 0.5F;

        MoveRelative(0, 1, OnGround ? 0.07F : 0.01F);
        YDelta = (float)(YDelta - 0.08);
        Move(XDelta, YDelta, ZDelta);
        XDelta *= 0.91F;
        YDelta *= 0.98F;
        ZDelta *= 0.91F;
        if (OnGround)
        {
            XDelta *= 0.7F;
            ZDelta *= 0.7F;
        }
    }

    public override void Render(float a)
    {
        var time = Time.GetTicksMsec() / 1000.0 * 10.0 * _speed + _timeOffs;
        _renderer.Update(time);
    }
}