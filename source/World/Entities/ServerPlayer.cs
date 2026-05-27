using TestClient.Source.Render.Model;
using TestClient.Source.Render.Model.impl;

namespace TestClient.Source.World.Entities;

public class ServerPlayer : ServerEntity
{
    public ServerPlayer(Level level) : base(level)
    {
    }

    public override void LivingTick()
    {
        GetModel<BipedModel>().IsSneak = IsSneaking();
        base.LivingTick();
    }

    protected override T GetModelRenderer<T>()
    {
        return (T)new ModelRenderer(new BipedModel(), "res://assets/entity/steve.png", Game.Singleton, 64, 64);
    }
}