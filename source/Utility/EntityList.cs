using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Utility;

public class EntityList
{
    public static Entity CreateEntityByID(int packetInEntityType, Level singletonLevel)
    {
        return packetInEntityType switch
        {
            54 => new Zombie(singletonLevel),
            _ => null
        };
    }
}