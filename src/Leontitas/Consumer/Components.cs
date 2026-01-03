using Leontitas;

namespace Consumer;

[Game] public struct Id : IComponent { public int Value; }
[Game] public struct Hp : IComponent { public int Value; }
[Game] public struct Alive : IComponent { }

[Game]
public struct Quaternion : IComponent
{
    public float X;
    public float Y;
    public float Z;
    public float W;
}
