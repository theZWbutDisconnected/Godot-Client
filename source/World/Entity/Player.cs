using Godot;

namespace TestClient.Source.World.Entity;

public partial class Player : Entity
{
    public Player(Level level) : base(level)
    {
    }

    public override void Tick()
    {
        base.Tick();

        var xa = 0.0F;
        var ya = 0.0F;

        if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W)) --ya;
        if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S)) ++ya;
        if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A)) --xa;
        if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D)) ++xa;
        if ((Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Bracketright)) && OnGround) YDelta = 0.5F;

        MoveRelative(xa, ya, OnGround ? 0.1F : 0.02F);
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
}