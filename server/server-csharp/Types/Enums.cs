using SpacetimeDB;

namespace SnowFight.Types
{
    [SpacetimeDB.Type]
    public enum PlayerActions
    {
        Throw,
        Hit,
        Craft,
        Standup,
    }
    
    [SpacetimeDB.Type]
    public enum ColliderType 
    {
        Box,
        Sphere
    }
} 