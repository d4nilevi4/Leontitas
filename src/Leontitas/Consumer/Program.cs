using Leontitas;

namespace Consumer;

class Program
{
    static void Main(string[] args)
    {
        GameWorld world = GameWorld.Create();
        
        GameEntity entity1 = world.CreateEntity()
            .AddId(1);
        
        GameEntity entity2 = world.CreateEntity()
            .AddId(2)
            .AddQuaternion(1, 1, 1, 1);
       
        GameGroup group = world.GetGroup(GameMatcher
            .AllOf(GameMatcher.Id)
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
        
        GameWorld.Destroy();
    }
}