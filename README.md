# Leontitas

**Type-Safe Entitas-Style Code Generator for LeoEcsLite**

![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black) ![.NET](https://img.shields.io/badge/.NET-8.0-purple) ![License](https://img.shields.io/github/license/d4nilevi4/Leontitas) [![LeoEcsLite](https://img.shields.io/badge/LeoEcsLite-AA?style=flat&logo=github)](https://github.com/Leopotam/ecslite)

> Leontitas is a **non-invasive code generation add-on** for LeoEcsLite that brings Entitas-style syntax to your ECS projects.
> Simply declare your worlds and components with attributes - no changes to your existing code required.
> Get a complete type-safe, fluent API generated at compile time, combining LeoEcsLite's lightweight nature with Entitas-inspired developer experience.

## Navigation

- [Motivation](#motivation)
- [What Leontitas IS and IS NOT](#what-leontitas-is-and-is-not)
- [Who might find this interesting?](#who-might-find-this-interesting)
- [Key Features](#key-features)
- [Instalation](#instalation)
- [Quick start](#quick-start)
- [Usage guide](#usage-guide)
  - [World Management](#world-management)
  - [Component Patterns](#component-patterns)
  - [Entity Operations](#entity-operations)
  - [Querying Entities](#querying-entities)
  - [Pool Access (Advanced)](#pool-access-advanced)
  - [LeoEcsLite Systems Integration](#leoecslite-systems-integration)
  - [Leontitas.Systems Integration](#leontitassystems-integration)
- [API reference](#api-reference)
  - [Generated Types Per World](#generated-types-per-world)
  - [Component-Specific Methods](#component-specific-methods)
  - [World API](#world-api)
  - [Entity API](#entity-api)
  - [Matcher API](#matcher-api)
  - [Pool API](#pool-api)
- [Project structure](#project-structure)

## Motivation

### The Genesis

This project started as a **learning journey** to deeply understand two things:
1. **C# Source Generators** - Roslyn's compile-time code generation capabilities
2. **LeoEcsLite** - How this lightweight ECS framework works under the hood

Initially, the goal was simple: dive into LeoEcsLite's internals, understand its architecture, and learn by exploring. Through this process, a new idea emerged.

### The Idea

While studying LeoEcsLite, I noticed it shares the same core concepts as Entitas but with different syntax. Both are excellent ECS frameworks:

- **LeoEcsLite**: Lightweight, fast, minimal allocations - but has a steeper learning curve and requires manual pool management
- **Entitas**: Beginner-friendly developer experience - but heavier and uses runtime code generation

**The question arose:** What if we could combine them? Take LeoEcsLite's lightweight performance and wrap it with Entitas-style syntax using compile-time code generation?

This became the core concept of Leontitas: **a bridge between two worlds**.

Leontitas uses **Roslyn source generators** to analyze your component declarations and automatically generate a complete Entitas-style API at compile time. The result is a non-invasive add-on that doesn't modify your existing code - it simply generates new APIs alongside it.

### Key goals:
- **Learning exercise** - Practical exploration of source generators and ECS internals
- **Bridge frameworks** - Combine LeoEcsLite's efficiency with Entitas's ergonomics
- **Non-invasive** - Add-on approach that doesn't change existing code
- **Compile-time generation** - All wrapper code generated during build
  
## What Leontitas IS and IS NOT

Leontitas is a **source code generator** for the LeoEcsLite Entity Component System framework. It analyzes your code at compile time and generates type-safe, Entitas-style wrapper APIs around LeoEcsLite's core.

### What Leontitas IS

- A **non-invasive code generation add-on** for LeoEcsLite
- A **learning project** exploring source generators and ECS architecture
- A **bridge** between LeoEcsLite's performance and Entitas's developer experience
- An **experiment** in combining two excellent frameworks
- A **compile-time tool** - generates code during build, not at runtime
- An **extension** - works on top of LeoEcsLite, doesn't modify it
- **Compatible with raw LeoEcsLite** - mix both APIs freely

### What Leontitas IS NOT

- **NOT a replacement** for LeoEcsLite (it builds on top of it)
- **NOT a performance improvement** (it's a syntax wrapper with minimal overhead)
- **NOT a replacement** for Entitas (different architecture, different goals)
- **NOT production-battle-tested** - it's a learning/exploration project
- **NOT a runtime framework** - no runtime code generation or reflection

### What's Missing from Entitas

Leontitas focuses on providing Entitas-style syntax for basic ECS operations, but **does not include** several advanced Entitas features:

**Not implemented (and unlikely to be added):**
- **Reactive Systems** - No `IReactiveSystem`, `OnEntityAdded`, `OnEntityRemoved` callbacks. These add runtime complexity and go against LeoEcsLite's minimalist philosophy. If you need reactivity, consider implementing it manually in your systems or using LeoEcsLite's raw filters.
- **Entity Events** - No automatic event firing on entity creation/destruction. This would require runtime overhead that conflicts with the zero-cost abstraction goal.
- **AnyOf Matcher** - No `IAnyOfMatcher` or `AnyOf` filtering support. LeoEcsLite doesn't provide `AnyOf` functionality under the hood - it only supports `AllOf` (Inc) and `NoneOf` (Exc) operations. Adding `AnyOf` would require building a custom filtering layer on top, which contradicts the goal of staying close to LeoEcsLite's architecture.

**Not implemented (might be added in the future):**
- **Component Attributes** - No `[PrimaryEntityIndex]`, `[Unique]`, `[EntityIndex]` attributes for advanced indexing and lookups. These could potentially be implemented as they align with compile-time generation, but would require significant design work.

**Why these omissions?**

Leontitas is a learning project exploring the intersection of source generators and ECS. Adding reactive systems and events would:
- Introduce runtime overhead (contradicting the "zero overhead" goal)
- Add significant complexity to the generator code
- Move away from LeoEcsLite's minimalist design philosophy
- Require runtime infrastructure beyond simple wrapper generation

If you need these features, consider using full Entitas or implementing them manually on top of LeoEcsLite's raw API (which you can mix with Leontitas code freely).

## Who might find this interesting?

- **Developers learning source generators** - See a practical, real-world implementation of Roslyn source generators
- **LeoEcsLite users** - Want more ergonomic APIs without changing frameworks or learning new systems
- **Entitas users** - Curious about a lighter alternative with familiar syntax and compile-time generation
- **ECS enthusiasts** - Interested in bridging different ECS approaches and exploring architecture patterns
- **Unity developers** - Looking for cleaner ECS code with compile-time safety and better IntelliSense support
- **Students and learners** - Want to understand how code generation works in a practical project

## Key Features

- **Compile-time code generation** - All wrapper code is generated during build using Roslyn source generators. No runtime overhead, no reflection.

- **Entitas-style fluent API** - Write clean, chainable code like `entity.AddPosition(x, y).AddVelocity(dx, dy)` instead of manual pool management.

- **Full IntelliSense support** - Your IDE knows about every component and operation. Get autocomplete, go-to-definition, and compile-time error checking.

- **Works with Unity and .NET** - Available as Unity Package Manager package (2021.3+) or standalone .NET 8.0 library. Compatible with IL2CPP/AOT.

- **Multiple API styles** - Choose what fits your needs: entity-centric (`entity.AddHp(100)`), pool-centric (`hpPool.Add(entity)`), or query-based (`GameMatcher.AllOf(...)`).

- **Smart component handling** - Flag components become boolean properties, single-field components get direct accessors, multi-field components use method parameters.

## Instalation

### Requirements

* Requires Unity version 2021.3 or higher.
* Requires ecslite package installed. Guide regarding ecslite installation can be found
  on [Leopotam/ecslite README](https://github.com/Leopotam/ecslite?tab=readme-ov-file#%D0%A3%D1%81%D1%82%D0%B0%D0%BD%D0%BE%D0%B2%D0%BA%D0%B0).

### Unity Installation

Leontitas consists of two packages:

| Package | Status | Description |
|---------|--------|-------------|
| **Leontitas Core** | **Required** | Main code generator and fluent API |
| **Leontitas.Systems** | *Optional* | Entitas-style system lifecycle (`IInitializeSystem`, `IExecuteSystem`, etc.) |

---

#### Installing Leontitas Core (Required)

**Option 1: Via Git URL (UPM)**

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from git URL...`
3. Enter:
   ```
   https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Core
   ```
4. Click `Add`

**Option 2: Via manifest.json**

Add to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.d4nilevi4.leontitas": "https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Core"
  }
}
```

---

#### Installing Leontitas.Systems (Optional)

**If you want Entitas-style system lifecycle management**, install this additional package:

**Option 1: Via Git URL (UPM)**

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from git URL...`
3. Enter:
   ```
   https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Systems
   ```

**Option 2: Via manifest.json**

Add to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.d4nilevi4.leontitas": "https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Core",
    "com.d4nilevi4.leontitas.systems": "https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Systems"
  }
}
```

**What does Leontitas.Systems provide?**
- `IInitializeSystem`, `IExecuteSystem`, `ICleanupSystem`, `ITearDownSystem` interfaces
- `Feature` class for organizing systems into groups
- Clean lifecycle management similar to Entitas

See [Leontitas.Systems Integration](#leontitassystems-integration) section for detailed usage examples.

## Quick start

### Step 1: Declare Your World

Create a new C# file and declare a world using an assembly attribute:

```csharp
using Leontitas;

[assembly: WorldDeclaration("Game")]
```

This tells Leontitas to generate a complete API for a world named "Game".

**Generated classes:**
- `GameWorld` - World singleton wrapper
- `GameEntity` - Entity handle with fluent API
- `GamePool<T>` - Type-safe pool access
- `GameMatcher` - Query builder
- `GameGroup` - Query results container

---

### Step 2: Define Your Components

Create components by implementing `IComponent` and marking them with the world attribute:

```csharp
using Leontitas;

// Multi-field component
[Game]
public struct Position : IComponent
{
    public float X;
    public float Y;
}

// Multi-field component
[Game]
public struct Velocity : IComponent
{
    public float Dx;
    public float Dy;
}

// Single-field component (generates property accessor)
[Game]
public struct Health : IComponent
{
    public int Value;
}

// Flag component (no fields - generates boolean property)
[Game]
public struct Movable : IComponent { }

[Game]
public struct Player : IComponent { }
```

**Component Rules:**
- Must be a `struct`
- Must implement `IComponent`
- Must have world attribute (`[Game]`, `[Input]`, etc.)
- Can have 0+ fields (public fields only)

---

### Step 3: Use the Generated API

```csharp
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private GameWorld _world;
    private GameGroup _movables;
    private GameGroup _players;

    private void Start()
    {
        // Create the world
        _world = GameWorld.Create();

        // Create entities with fluent API
        var player = GameEntity.Create()
            .AddPosition(0f, 0f)
            .AddVelocity(5f, 0f)
            .AddHealth(100)
            .SetMovableFlag(true)
            .SetPlayerFlag(true);

        var enemy = GameEntity.Create()
            .AddPosition(10f, 5f)
            .AddHealth(50)
            .SetMovableFlag(true);

        Debug.Log($"Player position: ({player.Position.X}, {player.Position.Y})");
        Debug.Log($"Player health: {player.Health}");

        // Query movable entities
        _movables = _world.GetGroup(
            GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Velocity)
        );

        // Query players specifically
        _players = _world.GetGroup(
            GameMatcher.AllOf(GameMatcher.Player)
        );

    }

    private void Update()
    {
        foreach (var entity in _movables)
        {
            // Move entity
            entity.ChangePosition(
                entity.Position.X + entity.Velocity.Dx * Time.deltaTime,
                entity.Position.Y + entity.Velocity.Dy * Time.deltaTime
            );
        }

        foreach (var player in _players)
        {
            if (player.Health <= 0)
            {
                player.Destroy();
            }
        }
    }

    void OnDestroy()
    {
        // Clean up
        _world.Destroy();
    }
}
```

**That's it!** No manual pool management, no boilerplate. Just clean ECS code.

## Usage guide

### World Management

**Creating Worlds:**
```csharp
// Create singleton instance
var world = GameWorld.Create();

// Access instance later
var world = GameWorld.Instance;

// Destroy when done
world.Destroy();
```

**Multiple Worlds:**
```csharp
// Declare multiple worlds
[assembly: WorldDeclaration("Game")]
[assembly: WorldDeclaration("UI")]
[assembly: WorldDeclaration("Input")]

// Each gets its own API
var gameWorld = GameWorld.Create();
var uiWorld = UIWorld.Create();
var inputWorld = InputWorld.Create();

// Destroy when needed
gameWorld.Destroy();
uiWorld.Destroy();
inputWorld.Destroy();

// Components are world-specific
[Game, UI, Input] public struct Id : IComponent { ... }
[Game] public struct Position : IComponent { ... }
[UI] public struct ButtonState : IComponent { ... }
[Input] public struct PlayerInput : IComponent { ... }
```

---

### Component Patterns

#### Flag Components (0 fields)

**Definition:**
```csharp
[Game] public struct Movable : IComponent { }
```

**Generated API:**
```csharp
// Check presence (property)
if (entity.IsMovable) { ... }

// Set/unset (method)
entity.SetMovableFlag(true);   // Add flag
entity.SetMovableFlag(false);  // Remove flag

// Alternative
entity.IsMovable = true;
entity.IsMovable = false;
```

---

#### Single-Field Components

**Definition:**
```csharp
[Game] public struct Health : IComponent
{
    public int Value;
}
```

**Generated API:**
```csharp
// Property accessor (read)
int currentHp = entity.Health;

// Reference property
ref Health hp = ref entity.HealthRef;

// Existing component flag
bool hasHp = entity.HasHealth;

// Add component
entity.AddHealth(100);

// Change existing
entity.ChangeHealth(entity.Health - 10);

// Replace (Add or update)
entity.ReplaceHealth(50);

// Remove
entity.RemoveHealth();
```

---

#### Multi-Field Components

**Definition:**
```csharp
[Game] public struct Position : IComponent
{
    public float X;
    public float Y;
    public float Z;
}
```

**Generated API:**
```csharp
// Reference property
ref Position pos = ref entity.PositionRef;

// Existing component flag
bool hasPosition = entity.HasPosition;

// Add (all fields as parameters)
entity.AddPosition(0f, 0f, 0f);

// Add (from existing struct)
var newPos = new Position { X = 0f, Y = 0f, Z = 0f };
entity.AddPosition(in newPos);

// Change (update existing with individual values)
entity.ChangePosition(
    entity.PositionRef.X + 1,
    entity.PositionRef.Y,
    entity.PositionRef.Z
);

// Change (update from struct)
var updatedPos = entity.PositionRef;
updatedPos.X += 1;
entity.ChangePosition(in updatedPos);

// Replace (add or update with individual values)
entity.ReplacePosition(10f, 5f, 0f);

// Replace (add or update from struct)
var replacePos = new Position { X = 10f, Y = 5f, Z = 0f };
entity.ReplacePosition(in replacePos);

// Remove
entity.RemovePosition();
```

---

### Entity Operations

**Creating Entities:**
```csharp
// Simple creation
var entity = GameEntity.Create();

// Fluent creation
var enemy = GameEntity.Create()
    .AddPosition(x, y, z)
    .AddHealth(50)
    .SetMovableFlag(true);
```

**Component Operations:**
```csharp
// Add (throws if already present)
entity.AddHealth(100);

// Change (throws if not present)
entity.ChangeHealth(entity.Health - 10);

// Replace (safe - adds if missing, updates if present)
entity.ReplaceHealth(50);

// Remove
entity.RemoveHealth();

// Has (check)
if (entity.HasHealth) { ... }
```

**Destroying Entities:**
```csharp
entity.Destroy();
```

---

### Querying Entities

**Basic Queries:**
```csharp
// All entities with specific components
var movables = GameWorld.Instance.GetGroup(
    GameMatcher.AllOf(GameMatcher.Movable, GameMatcher.Velocity)
);

foreach (var entity in movables)
{
    // Process...
}
```

**Complex Queries:**
```csharp
// AllOf + NoneOf (entities WITH A and B but WITHOUT C)
var aliveEnemies = GameWorld.Instance.GetGroup(
    GameMatcher.AllOf(GameMatcher.Enemy, GameMatcher.Health)
                .NoneOf(GameMatcher.Dead)
);
```

---

### Pool Access (Advanced)

For bulk operations or performance-critical code, use pools directly:

```csharp
// Get typed pool
var positionPool = GameWorld.Instance.GetGamePool<Position>();
var velocityPool = GameWorld.Instance.GetGamePool<Velocity>();

// Pool operations
ref var pos = ref positionPool.Add(entity);
pos.X = 10;
pos.Y = 5;

if (positionPool.Has(entity))
{
    ref var p = ref positionPool.Get(entity);
    p.X += 1;
}

positionPool.Remove(entity);
```

**When to use pools:**
- Bulk operations on many entities
- Hot paths where method call overhead matters
- When you need `ref` access for zero-copy mutations
- Integration with raw LeoEcsLite systems

---

### LeoEcsLite Systems Integration

Leontitas works seamlessly with LeoEcsLite's system pipeline:

```csharp
public class MovementSystem : IEcsRunSystem
{
    private GameGroup _movables;

    public MovementSystem()
    {
        // Use Leontitas API in systems
        _movables = GameWorld.Instance.GetGroup(GameMatcher
            .AllOf(
                GameMatcher.Position,
                GameMatcher.Velocity,
                GameMatcher.Movable));
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _movables)
        {
            entity.ChangePosition(
                entity.Position.X + entity.Velocity.Dx * Time.deltaTime,
                entity.Position.Y + entity.Velocity.Dy * Time.deltaTime,
                entity.Position.Z
            );
        }
    }
}

public class HealthSystem : IEcsRunSystem
{
    private GameGroup _entities;

    public HealthSystem()
    {
        _entities = GameWorld.Instance.GetGroup(GameMatcher
            .AllOf(
                GameMatcher.Health));
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _entities)
        {
            if (entity.Health <= 0)
            {
                entity.SetDeadFlag(true);
            }
        }
    }
}

// Setup
var systems = new EcsSystems(GameWorld.Instance.World);
systems
    .Add(new MovementSystem())
    .Add(new HealthSystem())
    .Init();

// In update loop
systems.Run();
```

---

### Leontitas.Systems Integration

For a more modular and Unity-friendly approach, you can use the **Leontitas.Systems** package. This is a separate optional package that provides Entitas-style system lifecycle management.

**Installation:**

```
https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Systems
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.d4nilevi4.leontitas.systems": "https://github.com/d4nilevi4/Leontitas.git?path=src/Leontitas.Unity/Systems"
  }
}
```

**System Interfaces:**

```csharp
public interface IInitializeSystem : ISystem
{
    void Initialize();  // Called once when system starts
}

public interface IExecuteSystem : ISystem
{
    void Execute();  // Called every frame (Update)
}

public interface ICleanupSystem : ISystem
{
    void Cleanup();  // Called after Execute for cleanup logic
}

public interface ITearDownSystem : ISystem
{
    void TearDown();  // Called when system is destroyed
}
```

**Example:**

```csharp
using Leontitas;
using UnityEngine;

public class MovementSystem : IInitializeSystem, IExecuteSystem
{
    private GameGroup _movables;

    public void Initialize()
    {
        _movables = GameWorld.Instance.GetGroup(
            GameMatcher.AllOf(GameMatcher.Position, GameMatcher.Velocity, GameMatcher.Movable)
        );
    }

    public void Execute()
    {
        foreach (var entity in _movables)
        {
            entity.ChangePosition(
                entity.Position.X + entity.Velocity.Dx * Time.deltaTime,
                entity.Position.Y + entity.Velocity.Dy * Time.deltaTime,
                entity.Position.Z
            );
        }
    }
}

public class DamageSystem : IExecuteSystem, ICleanupSystem
{
    private GameGroup _damagedEntities;

    public void Initialize()
    {
        _damagedEntities = GameWorld.Instance.GetGroup(
            GameMatcher.AllOf(GameMatcher.Damage, GameMatcher.Health)
        );
    }

    public void Execute()
    {
        foreach (var entity in _damagedEntities)
        {
            entity.ReplaceHealth(entity.Health - entity.Damage);
        }
    }

    public void Cleanup()
    {
        // Remove damage components after processing
        foreach (var entity in _damagedEntities)
        {
            entity.RemoveDamage();
        }
    }
}
```

**Using Features (System Groups):**

```csharp
using Leontitas;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameWorld _world;
    private Feature _gameplayFeature;

    void Start()
    {
        _world = GameWorld.Create();

        // Create a feature (system group)
        _gameplayFeature = new Feature();
        _gameplayFeature.Add(new MovementSystem());
        _gameplayFeature.Add(new DamageSystem());
        _gameplayFeature.Add(new HealthSystem());

        // Initialize all systems
        _gameplayFeature.Initialize();
    }

    void Update()
    {
        // Execute all systems
        _gameplayFeature.Execute();

        // Cleanup after execution
        _gameplayFeature.Cleanup();
    }

    void OnDestroy()
    {
        // Tear down systems
        _gameplayFeature.TearDown();

        _world.Destroy();
    }
}
```

## API reference

### Generated Types Per World

For each world declared with `[assembly: WorldDeclaration("WorldName")]`, Leontitas generates:

| Type | Description | Example Usage |
|------|-------------|---------------|
| `{WorldName}World` | Singleton world wrapper | `GameWorld.Create()` |
| `{WorldName}Entity` | Type-safe entity handle (ref struct) | `GameEntity.Create()` |
| `{WorldName}Pool<T>` | Type-safe pool wrapper | `world.GetGamePool<Health>()` |
| `{WorldName}Matcher` | Query builder with component indices | `GameMatcher.AllOf(...)` |
| `{WorldName}Group` | Query result container | `foreach (var e in group)` |
| `{WorldName}ComponentsLookup` | Component index registry | *(Internal use)* |

### Component-Specific Methods

Generated methods depend on the number of fields:

#### 0 Fields (Flag Components)

```csharp
[Game] public struct Movable : IComponent { }
```

**Generates:**
- `bool IsMovable { get; }` - Check if component exists
- `SetMovableFlag(bool value)` - Add/remove flag

---

#### 1 Field (Property Components)

```csharp
[Game] public struct Health : IComponent { public int Value; }
```

**Generates:**
- `int Health { get; }` - Read property (returns `Value`)
- `bool HasHealth { get; }` - Returns existing component flag
- `ref Health HealthRef { get; }` - Direct reference to component
- `AddHealth(int value)` - Add component
- `ChangeHealth(int value)` - Update existing component
- `ReplaceHealth(int value)` - Add or update
- `RemoveHealth()` - Remove component

---

#### 2+ Fields (Method Components)

```csharp
[Game] public struct Position : IComponent
{
    public float X;
    public float Y;
}
```

**Generates:**
- `bool HasPosition { get; }` - Returns existing component flag
- `ref Position PositionRef { get; }` - Direct reference to component
- `AddPosition(float x, float y)` - Add component with individual field values
- `AddPosition(in Position component)` - Add component from existing struct
- `ChangePosition(float x, float y)` - Update existing component with individual values
- `ChangePosition(in Position component)` - Update existing component from struct
- `ReplacePosition(float x, float y)` - Add or update with individual values
- `ReplacePosition(in Position component)` - Add or update from struct
- `RemovePosition()` - Remove component

---

### World API

```csharp
namespace Leontitas;

public sealed partial class GameWorld : EcsWorld
{
    public static readonly int MaxComponentsCount = X;

    public static GameWorld Instance { get; }

    public static void Create(in EcsWorld.Config config);
    public static void Create();
    
    public void Destroy();

    public GameEntity CreateEntity();
    
    public GameGroup GetGroup(IAllOfGameMatcher matcher);
    public GameGroup GetGroup(INoneOfGameMatcher matcher);

    public GamePool<TComponent> GetGamePool<TComponent>() 
        where TComponent : struct, IComponent;
}
```

---

### Entity API

```csharp
namespace Leontitas;

public readonly ref partial struct GameEntity
{
    static GameEntity Create();

    public int InstanceId { get; }
    public void Destroy();

    // Component-specific methods generated per component...
    // (see Component-Specific Methods above)
}
```

---

### Matcher API

```csharp
namespace Leontitas;

public partial class GameMatcher
{
    public static GameMatcher AllOf(params int[] indices);
    public GameMatcher NoneOf(params int[] indices);

    // Component indices (one per component)
    public static int Position { get; }
    public static int Velocity { get; }
    public static int Health { get; }
    // ... etc
}
```

---

### Pool API

```csharp
namespace Leontitas;

public readonly struct GamePool<TComponent>
    where TComponent : struct, IComponent
{
    ref TComponent Add(GameEntity entity);
    ref TComponent Get(GameEntity entity);
    bool Has(GameEntity entity);
    void Remove(GameEntity entity);
}
```

## Project structure

```
Leontitas/
│
└── src/
    ├── Leontitas/                          # .NET Solution
    │   ├── Leontitas/                      # Runtime library
    │   ├── LeontitasCodeGenerator/         # Source generator
    │   ├── LeoEcsLite/                     # Embedded ECS framework
    │   ├── Consumer/                       # Example project
    │   ├── Benchmark/                      # Performance tests
    │   └── Leontitas.sln                   # Visual Studio solution
    │
    └── Leontitas.Unity/                    # Unity Packages
        ├── Core                            # Leontitas Core
        └── Systems                         # Systems for Leontitas
```

**Key Directories:**

- **`src/Leontitas/Leontitas/`** - Tiny runtime (2 files, ~50 lines total)
- **`src/Leontitas/LeontitasCodeGenerator/`** - Code generator (756 lines)
- **`src/Leontitas/LeoEcsLite/`** - Embedded ECS framework dependency
- **`src/Leontitas/Consumer/`** - Working example showing usage
- **`src/Leontitas/Benchmark/`** - Performance validation
- **`src/Leontitas.Unity/Core/`** - Leontitas.Core Unity package for distribution
- **`src/Leontitas.Unity/Systems/`** - Leontitas.Systems Unity package for distribution