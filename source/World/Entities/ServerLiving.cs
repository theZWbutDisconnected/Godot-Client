using TestClient.Source.Utility;

namespace TestClient.Source.World.Entities;

public abstract class ServerLiving : ServerEntity
{
    private EntityBodyHelper _bodyHelper;

    public ServerLiving(Level level) : base(level)
    {
        _bodyHelper = new EntityBodyHelper(this);
    }

    protected override float UpdateDistance(float p_110146_1_, float p_110146_2_)
    {
        _bodyHelper.UpdateRenderAngles();
        return p_110146_2_;
    }
}