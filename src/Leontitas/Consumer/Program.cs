using Leontitas;

namespace Consumer;

class Program
{
    static void Main(string[] args)
    {
        GameWorld.Create();

        GameEntity entity1 = GameWorld.CreateEntity()
            .AddId(1)
            .SetAliveFlag(true);

        GameEntity entity2 = GameWorld.CreateEntity()
            .AddId(2)
            .AddQuaternion(1, 1, 1, 1)
            .SetAliveFlag(true);

        GameGroup group = GameWorld.GetGroup(GameMatcher
            .AllOf(GameMatcher.Id, GameMatcher.Alive)
            .NoneOf(GameMatcher.Quaternion));

        foreach (GameEntity gameEntity in group)
        {
            Console.WriteLine("Entity Id without Quaternion: " + gameEntity.IdRef.Value);
        }

        entity1.Destroy();

        foreach (GameEntity gameEntity in group)
        {
            Console.WriteLine("Entity Id without Quaternion: " + gameEntity.IdRef.Value);
        }

        if (GameWorld.IsAlive())
            GameWorld.Destroy();
    }
}