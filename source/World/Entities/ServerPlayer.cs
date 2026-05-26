using TestClient.Source.Render.Model;
using TestClient.Source.Render.Model.impl;

namespace TestClient.Source.World.Entities;

public class ServerPlayer : ServerEntity
{

    public ServerPlayer(Level level) : base(level)
    {
    }
    
    protected override ModelRenderer GetModelRenderer()
    {
        return Renderer ??= new ModelRenderer(new BipedModel(), "res://assets/entity/steve.png", Game.Singleton, 64, 64);
    }
}