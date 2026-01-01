namespace Leontitas
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class WorldDeclarationAttribute : System.Attribute
    {
        public string WorldName { get; }

        public WorldDeclarationAttribute(string worldName)
        {
            WorldName = worldName;
        }
    }
}