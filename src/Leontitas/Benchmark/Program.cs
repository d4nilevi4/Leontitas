using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Consumer;
using GameEntity = Leontitas.GameEntity;
using GameGroup = Leontitas.GameGroup;
using GameMatcher = Leontitas.GameMatcher;

namespace Benchmark;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}

[MemoryDiagnoser]
public class Benchmark
{
    [Params(10000)] public int EntityCount;

    [Params(100000)] public int Runs;

    [Benchmark]
    public void LeontitasChangeMethod()
    {
        Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        int currentId = 0;
        foreach (GameEntity gameEntity in idGroup)
        {
            gameEntity.ChangeId(currentId++);
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity gameEntity in hpGroup)
            {
                gameEntity.ChangeHp(gameEntity.Hp + 1);
            }
        }

        Leontitas.GameWorld.Destroy();
    }

    [Benchmark]
    public void LeontitasReplaceMethod()
    {
        Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        int currentId = 0;
        foreach (GameEntity gameEntity in idGroup)
        {
            gameEntity.ReplaceId(currentId++);
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity gameEntity in hpGroup)
            {
                gameEntity.ReplaceHp(gameEntity.Hp + 1);
            }
        }

        Leontitas.GameWorld.Destroy();
    }

    [Benchmark]
    public void LeontitasWithPools()
    {
        Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = Leontitas.GameWorld.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        Leontitas.GamePool<Id> idPool = Leontitas.GameWorld.GetGamePool<Id>();
        Leontitas.GamePool<Hp> hpPool = Leontitas.GameWorld.GetGamePool<Hp>();


        int currentId = 0;
        foreach (GameEntity gameEntity in idGroup)
        {
            idPool.Get(gameEntity).Value = currentId++;
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity gameEntity in hpGroup)
            {
                hpPool.Get(gameEntity).Value++;
            }
        }

        Leontitas.GameWorld.Destroy();
    }

    [Benchmark]
    public void LeoEcsLite()
    {
        Leopotam.EcsLite.EcsWorld.Config config = new()
        {
            Entities = EntityCount,
        };
        
        Leopotam.EcsLite.EcsWorld world = new Leopotam.EcsLite.EcsWorld(in config);

        Leopotam.EcsLite.EcsPool<Id> idPool = world.GetPool<Id>();
        Leopotam.EcsLite.EcsPool<Hp> hpPool = world.GetPool<Hp>();


        for (int i = 0; i < EntityCount; i++)
        {
            int newEntity = world.NewEntity();
            idPool.Add(newEntity);
            hpPool.Add(newEntity);
        }

        Leopotam.EcsLite.EcsFilter idFilter = world.Filter<Id>().End();
        Leopotam.EcsLite.EcsFilter hpFilter = world.Filter<Hp>().End();

        int currentId = 0;
        foreach (int gameEntity in idFilter)
        {
            idPool.Get(gameEntity).Value = currentId++;
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (int gameEntity in hpFilter)
            {
                hpPool.Get(gameEntity).Value++;
            }
        }

        world.Destroy();
    }
}