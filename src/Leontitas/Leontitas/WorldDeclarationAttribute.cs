namespace Leontitas;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class WorldDeclarationAttribute : Attribute
{
    public string WorldName { get; }

    public WorldDeclarationAttribute(string worldName)
    {
        WorldName = worldName;
    }
}