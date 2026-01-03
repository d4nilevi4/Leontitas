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
        Leontitas.GameWorld world = Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        int currentId = 0;
        foreach (GameEntity GameEntity in idGroup)
        {
            GameEntity.ChangeId(currentId++);
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity GameEntity in hpGroup)
            {
                GameEntity.ChangeHp(GameEntity.Hp + 1);
            }
        }

        world.Destroy();
    }

    [Benchmark]
    public void LeontitasReplaceMethod()
    {
        Leontitas.GameWorld world = Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        int currentId = 0;
        foreach (GameEntity GameEntity in idGroup)
        {
            GameEntity.ReplaceId(currentId++);
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity GameEntity in hpGroup)
            {
                GameEntity.ReplaceHp(GameEntity.Hp + 1);
            }
        }

        world.Destroy();
    }

    [Benchmark]
    public void LeontitasWithPools()
    {
        Leontitas.GameWorld world = Leontitas.GameWorld.Create();

        for (int i = 0; i < EntityCount; i++)
        {
            GameEntity.Create()
                .AddId(0)
                .AddHp(0);
        }

        GameGroup idGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Id));
        GameGroup hpGroup = world.GetGroup(GameMatcher.AllOf(GameMatcher.Hp));

        Leontitas.GamePool<Id> idPool = world.GetGamePool<Id>();
        Leontitas.GamePool<Hp> hpPool = world.GetGamePool<Hp>();


        int currentId = 0;
        foreach (GameEntity GameEntity in idGroup)
        {
            idPool.Get(GameEntity).Value = currentId++;
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (GameEntity GameEntity in hpGroup)
            {
                hpPool.Get(GameEntity).Value++;
            }
        }

        world.Destroy();
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
        foreach (int GameEntity in idFilter)
        {
            idPool.Get(GameEntity).Value = currentId++;
        }

        for (int i = 0; i < Runs; i++)
        {
            foreach (int GameEntity in hpFilter)
            {
                hpPool.Get(GameEntity).Value++;
            }
        }

        world.Destroy();
    }
}