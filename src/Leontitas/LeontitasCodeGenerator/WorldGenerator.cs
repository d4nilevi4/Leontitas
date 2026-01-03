#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LeontitasCodeGenerator;

[Generator]
public class WorldGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var worldDeclarations = context.CompilationProvider
            .Select((compilation, _) => GetWorldNames(compilation));

        var componentStructs = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is StructDeclarationSyntax structDecl && structDecl.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetStructInfo(ctx))
            .Where(static info => info != null);

        var combined = worldDeclarations.Combine(componentStructs.Collect());

        context.RegisterSourceOutput(combined,
            (spc, data) => GenerateWorlds(data.Left, data.Right, spc));
    }

    private static ImmutableArray<string> GetWorldNames(Compilation compilation)
    {
        return compilation.Assembly
            .GetAttributes()
            .Where(attr => attr.AttributeClass?.Name == "WorldDeclarationAttribute")
            .Select(attr => attr.ConstructorArguments[0].Value?.ToString())
            .Where(name => !string.IsNullOrEmpty(name))
            .ToImmutableArray()!;
    }

    private static StructInfo? GetStructInfo(GeneratorSyntaxContext context)
    {
        var structDecl = (StructDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(structDecl) as INamedTypeSymbol;
        
        if (symbol == null)
            return null;

        var implementsIComponent = symbol.Interfaces.Any(i => i.Name == "IComponent");
        if (!implementsIComponent)
            return null;

        var worldAttributeNames = new List<string>();
        
        foreach (AttributeListSyntax attrList in structDecl.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var attrName = attr.Name.ToString();
                
                if (attrName.EndsWith("Attribute"))
                {
                    attrName = attrName.Substring(0, attrName.Length - "Attribute".Length);
                }
                
                if (attrName != "Serializable" && attrName != "Obsolete" && attrName != "Flags")
                {
                    worldAttributeNames.Add(attrName);
                }
            }
        }

        if (worldAttributeNames.Count == 0)
            return null;

        var fields = new List<ComponentField>();
        foreach (var member in symbol.GetMembers())
        {
            if (member is IFieldSymbol field && field.DeclaredAccessibility == Accessibility.Public)
            {
                fields.Add(new ComponentField
                {
                    Name = field.Name,
                    Type = field.Type.ToDisplayString()
                });
            }
        }

        var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace 
            ? string.Empty 
            : symbol.ContainingNamespace.ToDisplayString();

        return new StructInfo
        {
            WorldNames = worldAttributeNames.ToArray(),
            ComponentName = symbol.Name,
            FullNamespace = namespaceName,
            Fields = fields.ToArray()
        };
    }

    private static void GenerateWorlds(ImmutableArray<string> worldNames,
        ImmutableArray<StructInfo?> components, SourceProductionContext context)
    {
        if (worldNames.IsEmpty)
            return;

        foreach (var worldName in worldNames)
        {
            var worldComponents = components
                .Where(c => c != null && c.WorldNames.Contains(worldName))
                .Select(c => c!)
                .ToArray();

            GenerateComponentAttribute(worldName, context);
            
            GenerateEntity(worldName, context);
            
            GeneratePackedEntity(worldName, context);
            
            GeneratePool(worldName, context);
            
            GenerateMatcher(worldName, context);
            
            GenerateGroup(worldName, context);
            
            GenerateComponentsLookup(worldName, worldComponents, context);
            
            GenerateWorldClass(worldName, worldComponents, context);
            
            GenerateComponents(worldName, worldComponents, context);
        }
    }

    private static void GenerateWorldClass(string worldName, StructInfo[] components, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public sealed partial class {worldName}World : Leopotam.EcsLite.EcsWorld");
        sb.AppendLine("    {");
        sb.AppendLine($"        public static readonly int MaxComponentsCount = {components.Length};");
        sb.AppendLine("        ");
        sb.AppendLine($"        public static {worldName}World Instance");
        sb.AppendLine("        {");
        sb.AppendLine("            get");
        sb.AppendLine("            {");
        sb.AppendLine("                 if(_instance == null && !_instance.IsAlive())");
        sb.AppendLine("                 {");
        sb.AppendLine("                     throw new System.Exception(\"GameWorld is not created or already destroyed. Use CreateGameWorld method to create it.\");");
        sb.AppendLine("                 }");
        sb.AppendLine("        ");
        sb.AppendLine("                 return _instance;");
        sb.AppendLine("            }");
        sb.AppendLine("         }");
        sb.AppendLine("        ");
        sb.AppendLine($"        private static {worldName}World _instance;");
        sb.AppendLine("        ");
        sb.AppendLine("        public " + worldName + "World(in Leopotam.EcsLite.EcsWorld.Config config) : base(in config)");
        sb.AppendLine("        {");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public static {worldName}World Create(in Leopotam.EcsLite.EcsWorld.Config config)");
        sb.AppendLine("        {");
        sb.AppendLine("            if(_instance != null && _instance.IsAlive())");
        sb.AppendLine("            {");
        sb.AppendLine("                throw new System.Exception(\"GameWorld is already created. Destroy it before creating a new one.\");");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine($"            _instance = new {worldName}World(in config);");
        sb.AppendLine("            return _instance;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public static {worldName}World Create()");
        sb.AppendLine("        {");
        sb.AppendLine("             Leopotam.EcsLite.EcsWorld.Config defaultConfig = default;");
        sb.AppendLine("             return Create(in defaultConfig);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public new static void Destroy()");
        sb.AppendLine("        {");
        sb.AppendLine("             ((Leopotam.EcsLite.EcsWorld)Instance).Destroy();");
        sb.AppendLine("             _instance = null;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Entity CreateEntity()");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {worldName}Entity(base.NewEntity());");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Pool<TComponent> Get{worldName}Pool<TComponent>() where TComponent : struct, IComponent");
        sb.AppendLine("        {");
        sb.AppendLine("            Leopotam.EcsLite.EcsPool<TComponent> ecsPool = base.GetPool<TComponent>();");
        sb.AppendLine($"            return new {worldName}Pool<TComponent>(ecsPool);");
        sb.AppendLine("        }");
        sb.AppendLine("        ");
        sb.AppendLine($"        public {worldName}Group GetGroup(IAllOf{worldName}Matcher matcher)");
        sb.AppendLine("        {");
        sb.AppendLine($"            {worldName}Matcher {worldName.ToLower()}Matcher = ({worldName}Matcher)matcher;");
        sb.AppendLine($"            int firstIndex = {worldName.ToLower()}Matcher.IncludeIndices[0];");
        sb.AppendLine($"            System.ReadOnlySpan<int> includeIndices = System.MemoryExtensions.AsSpan({worldName.ToLower()}Matcher.IncludeIndices, 1);");
        sb.AppendLine("            ");
        sb.AppendLine("            Leopotam.EcsLite.EcsWorld.Mask filterMask = FilterByComponentIndex(firstIndex);");
        sb.AppendLine();
        sb.AppendLine("            foreach (int includeIndex in includeIndices)");
        sb.AppendLine("            {");
        sb.AppendLine($"                filterMask = {worldName}MaskExtensions.IncludeByComponentIndex(filterMask, includeIndex);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine($"            foreach (int excludeIndex in {worldName.ToLower()}Matcher.ExcludeIndices)");
        sb.AppendLine("            {");
        sb.AppendLine($"                filterMask = {worldName}MaskExtensions.ExcludeByComponentIndex(filterMask, excludeIndex);");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine($"            return new {worldName}Group(filterMask.End());");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Group GetGroup(INoneOf{worldName}Matcher matcher)");
        sb.AppendLine("        {");
        sb.AppendLine($"            return this.GetGroup((IAllOf{worldName}Matcher)matcher);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        private Leopotam.EcsLite.EcsWorld.Mask FilterByComponentIndex(int index)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (index)");
        sb.AppendLine("            {");
        
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            var componentFullName = string.IsNullOrEmpty(component.FullNamespace)
                ? component.ComponentName
                : $"{component.FullNamespace}.{component.ComponentName}";
            sb.AppendLine($"                case {i}:");
            sb.AppendLine($"                    return this.Filter<{componentFullName}>();");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new System.Exception();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    public static class {worldName}MaskExtensions");
        sb.AppendLine("    {");
        sb.AppendLine("        public static Leopotam.EcsLite.EcsWorld.Mask IncludeByComponentIndex(this Leopotam.EcsLite.EcsWorld.Mask mask, int index)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (index)");
        sb.AppendLine("            {");
        
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            var componentFullName = string.IsNullOrEmpty(component.FullNamespace)
                ? component.ComponentName
                : $"{component.FullNamespace}.{component.ComponentName}";
            sb.AppendLine($"                case {i}:");
            sb.AppendLine($"                    return mask.Inc<{componentFullName}>();");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new System.Exception();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        ");
        sb.AppendLine("        public static Leopotam.EcsLite.EcsWorld.Mask ExcludeByComponentIndex(this Leopotam.EcsLite.EcsWorld.Mask mask, int index)");
        sb.AppendLine("        {");
        sb.AppendLine("            switch (index)");
        sb.AppendLine("            {");
        
        for (int i = 0; i < components.Length; i++)
        {
            var component = components[i];
            var componentFullName = string.IsNullOrEmpty(component.FullNamespace)
                ? component.ComponentName
                : $"{component.FullNamespace}.{component.ComponentName}";
            sb.AppendLine($"                case {i}:");
            sb.AppendLine($"                    return mask.Exc<{componentFullName}>();");
        }

        sb.AppendLine("                default:");
        sb.AppendLine("                    throw new System.Exception();");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}World.g.cs", sb.ToString());
    }

    private static void GenerateComponentAttribute(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine("    [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]");
        sb.AppendLine($"    public class {worldName}Attribute : System.Attribute");
        sb.AppendLine("    {");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Attribute.g.cs", sb.ToString());
    }

    private static void GenerateEntity(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public readonly ref partial struct {worldName}Entity");
        sb.AppendLine("    {");
        sb.AppendLine("        public readonly int InstanceId;");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Entity(int id)");
        sb.AppendLine("        {");
        sb.AppendLine("            InstanceId = id;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public static {worldName}Entity Create()");
        sb.AppendLine("        {");
        sb.AppendLine($"            return {worldName}World.Instance.CreateEntity();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public void Destroy()");
        sb.AppendLine("        {");
        sb.AppendLine($"            {worldName}World.Instance.DelEntity(this.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public Packed{worldName}Entity Pack()");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new Packed{worldName}Entity(this);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        public override string ToString()");
        sb.AppendLine("        {");
        sb.AppendLine("            var sb = new System.Text.StringBuilder();");
        sb.AppendLine($"            object[] components = new object[{worldName}World.MaxComponentsCount];");
        sb.AppendLine();
        sb.AppendLine($"            int componentsCount = {worldName}World.Instance.GetComponents(InstanceId, ref components);");
        sb.AppendLine();
        sb.AppendLine("            for (int i = 0; i < componentsCount; i++)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (i > 0)");
        sb.AppendLine("                {");
        sb.AppendLine("                    sb.Append(\", \");");
        sb.AppendLine("                }");
        sb.AppendLine("                sb.Append(components[i].ToString());");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            return sb.ToString();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Entity.g.cs", sb.ToString());
    }

    private static void GeneratePackedEntity(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public readonly struct Packed{worldName}Entity");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly int _instanceId;");
        sb.AppendLine("        private readonly int _gen;");
        sb.AppendLine();
        sb.AppendLine($"        public Packed{worldName}Entity({worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            _instanceId = entity.InstanceId;");
        sb.AppendLine($"            _gen = {worldName}World.Instance.GetEntityGen(entity.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public bool TryUnpack(out {worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            entity = default;");
        sb.AppendLine();
        sb.AppendLine($"            var world = {worldName}World.Instance;");
        sb.AppendLine("            if (!world.IsAlive())");
        sb.AppendLine("            {");
        sb.AppendLine("                return false;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            int currentGen;");
        sb.AppendLine("            try");
        sb.AppendLine("            {");
        sb.AppendLine("                currentGen = world.GetEntityGen(_instanceId);");
        sb.AppendLine("            }");
        sb.AppendLine("            catch (System.IndexOutOfRangeException)");
        sb.AppendLine("            {");
        sb.AppendLine("                return false;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (currentGen != _gen)");
        sb.AppendLine("            {");
        sb.AppendLine("                return false;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine($"            entity = new {worldName}Entity(_instanceId);");
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"Packed{worldName}Entity.g.cs", sb.ToString());
    }

    private static void GeneratePool(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public readonly struct {worldName}Pool<TComponent>");
        sb.AppendLine("        where TComponent : struct, IComponent");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly Leopotam.EcsLite.EcsPool<TComponent> _ecsPool;");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Pool(Leopotam.EcsLite.EcsPool<TComponent> ecsPool)");
        sb.AppendLine("        {");
        sb.AppendLine("            _ecsPool = ecsPool;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public ref TComponent Add({worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            return ref _ecsPool.Add(entity.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public ref TComponent Get({worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            return ref _ecsPool.Get(entity.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public bool Has({worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            return _ecsPool.Has(entity.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public void Remove({worldName}Entity entity)");
        sb.AppendLine("        {");
        sb.AppendLine("            _ecsPool.Del(entity.InstanceId);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Pool.g.cs", sb.ToString());
    }

    private static void GenerateMatcher(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public partial class {worldName}Matcher : IAllOf{worldName}Matcher, INoneOf{worldName}Matcher");
        sb.AppendLine("    {");
        sb.AppendLine("        public int[] IncludeIndices { get; private set; } = System.Array.Empty<int>();");
        sb.AppendLine("        public int[] ExcludeIndices { get; private set; } = System.Array.Empty<int>();");
        sb.AppendLine("        ");
        sb.AppendLine($"        public static IAllOf{worldName}Matcher AllOf(params int[] indices)");
        sb.AppendLine("        {");
        sb.AppendLine($"            {worldName}Matcher allOf{worldName}Matcher = new {worldName}Matcher();");
        sb.AppendLine("            ");
        sb.AppendLine($"            allOf{worldName}Matcher.IncludeIndices = indices;");
        sb.AppendLine("            ");
        sb.AppendLine($"            return allOf{worldName}Matcher;");
        sb.AppendLine("        }");
        sb.AppendLine("        ");
        sb.AppendLine($"        public INoneOf{worldName}Matcher NoneOf(params int[] indices)");
        sb.AppendLine("        {");
        sb.AppendLine("            ExcludeIndices = indices;");
        sb.AppendLine("            return this;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Matcher.g.cs", sb.ToString());

        sb.Clear();
        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public interface IAllOf{worldName}Matcher");
        sb.AppendLine("    {");
        sb.AppendLine($"        INoneOf{worldName}Matcher NoneOf(params int[] indices);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"IAllOf{worldName}Matcher.g.cs", sb.ToString());

        sb.Clear();
        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public interface INoneOf{worldName}Matcher");
        sb.AppendLine("    {");
        sb.AppendLine("        ");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"INoneOf{worldName}Matcher.g.cs", sb.ToString());
    }

    private static void GenerateGroup(string worldName, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public readonly struct {worldName}Group ");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly Leopotam.EcsLite.EcsFilter _filter;");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Group(Leopotam.EcsLite.EcsFilter filter)");
        sb.AppendLine("        {");
        sb.AppendLine("            _filter = filter;");
        sb.AppendLine("        }");
        sb.AppendLine("        ");
        sb.AppendLine($"        public {worldName}GroupEnumerator GetEnumerator()");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {worldName}GroupEnumerator(_filter);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Group.g.cs", sb.ToString());

        sb.Clear();
        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public ref struct {worldName}GroupEnumerator");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly Leopotam.EcsLite.EcsFilter _filter;");
        sb.AppendLine("        private Leopotam.EcsLite.EcsFilter.Enumerator _filterEnumerator;");
        sb.AppendLine();
        sb.AppendLine("        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine($"        internal {worldName}GroupEnumerator(Leopotam.EcsLite.EcsFilter filter)");
        sb.AppendLine("        {");
        sb.AppendLine("            _filter = filter;");
        sb.AppendLine("            _filterEnumerator = _filter.GetEnumerator();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("        public bool MoveNext()");
        sb.AppendLine("        {");
        sb.AppendLine("            return _filterEnumerator.MoveNext();");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {worldName}Entity Current");
        sb.AppendLine("        {");
        sb.AppendLine("            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine($"            get => new {worldName}Entity(_filterEnumerator.Current);");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public void Dispose()");
        sb.AppendLine("        {");
        sb.AppendLine($"            _filterEnumerator.Dispose();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}GroupEnumerator.g.cs", sb.ToString());
    }

    private static void GenerateComponentsLookup(string worldName, StructInfo[] components, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public static class {worldName}ComponentsLookup");
        sb.AppendLine("    {");

        for (int i = 0; i < components.Length; i++)
        {
            sb.AppendLine($"        public static readonly int {components[i].ComponentName} = {i};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}ComponentsLookup.g.cs", sb.ToString());
    }

    private static void GenerateComponents(string worldName, StructInfo[] components, SourceProductionContext context)
    {
        foreach (var component in components)
        {
            GenerateComponentExtensions(worldName, component, context);
        }

        var sb = new StringBuilder();
        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public partial class {worldName}Matcher");
        sb.AppendLine("    {");

        foreach (var component in components)
        {
            sb.AppendLine($"        public static int {component.ComponentName} => {worldName}ComponentsLookup.{component.ComponentName};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Matcher_ComponentIndices.g.cs", sb.ToString());
    }

    private static string GetComponentApiName(string componentName)
    {
        if (componentName.EndsWith("Component") && componentName.Length > "Component".Length)
        {
            return componentName.Substring(0, componentName.Length - "Component".Length);
        }
        return componentName;
    }

    private static void GenerateComponentExtensions(string worldName, StructInfo component, SourceProductionContext context)
    {
        var sb = new StringBuilder();
        var componentFullName = string.IsNullOrEmpty(component.FullNamespace)
            ? component.ComponentName
            : $"{component.FullNamespace}.{component.ComponentName}";
        var componentApiName = GetComponentApiName(component.ComponentName);
        var poolName = $"{component.ComponentName}Pool";

        sb.AppendLine("namespace Leontitas");
        sb.AppendLine("{");
        sb.AppendLine($"    public readonly ref partial struct {worldName}Entity");
        sb.AppendLine("    {");
        sb.AppendLine($"        private static {worldName}Pool<{componentFullName}> {poolName} =>");
        sb.AppendLine($"            {worldName}World.Instance.Get{worldName}Pool<{componentFullName}>();");
        sb.AppendLine();

        bool isFlag = component.Fields.Length == 0;

        if (isFlag)
        {
            sb.AppendLine($"        public bool Is{componentApiName}");
            sb.AppendLine("        {");
            sb.AppendLine("            get");
            sb.AppendLine("            {");
            sb.AppendLine($"                return {poolName}.Has(this);");
            sb.AppendLine("            }");
            sb.AppendLine("            set");
            sb.AppendLine("            {");
            sb.AppendLine("                if (value)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    if (!{poolName}.Has(this))");
            sb.AppendLine("                    {");
            sb.AppendLine($"                        {poolName}.Add(this);");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                {");
            sb.AppendLine($"                    if ({poolName}.Has(this))");
            sb.AppendLine("                    {");
            sb.AppendLine($"                        {poolName}.Remove(this);");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine($"        public {worldName}Entity Set{componentApiName}Flag(bool is{componentApiName})");
            sb.AppendLine("        {");
            sb.AppendLine($"            Is{componentApiName} = is{componentApiName};");
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
        }
        else if (component.Fields.Length == 1)
        {
            var field = component.Fields[0];
            sb.AppendLine($"        public ref {componentFullName} {componentApiName}Ref => ref {poolName}.Get(this);");
            sb.AppendLine();
            sb.AppendLine($"        public bool Has{componentApiName} => {poolName}.Has(this);");
            sb.AppendLine();
            sb.AppendLine($"        public {field.Type} {componentApiName} => {poolName}.Get(this).{field.Name};");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Add{componentApiName}({field.Type} new{field.Name})");
            sb.AppendLine("        {");
            sb.AppendLine($"            ref var component = ref {poolName}.Add(this);");
            sb.AppendLine($"            component.{field.Name} = new{field.Name};");
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Replace{componentApiName}({field.Type} new{field.Name})");
            sb.AppendLine("        {");
            sb.AppendLine($"            if ({poolName}.Has(this))");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Change{componentApiName}(new{field.Name});");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Add{componentApiName}(new{field.Name});");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Change{componentApiName}({field.Type} new{field.Name})");
            sb.AppendLine("        {");
            sb.AppendLine($"            ref var component = ref {poolName}.Get(this);");
            sb.AppendLine($"            component.{field.Name} = new{field.Name};");
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Remove{componentApiName}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            {poolName}.Remove(this);");
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine($"        public ref {componentFullName} {componentApiName}Ref => ref {poolName}.Get(this);");
            sb.AppendLine();
            sb.AppendLine($"        public bool Has{componentApiName} => {poolName}.Has(this);");
            sb.AppendLine();

            var parameters = string.Join(", ", component.Fields.Select(f => $"{f.Type} new{f.Name}"));

            sb.AppendLine($"        public {worldName}Entity Add{componentApiName}({parameters})");
            sb.AppendLine("        {");
            sb.AppendLine($"            ref {componentFullName} component = ref {poolName}.Add(this);");
            foreach (var field in component.Fields)
            {
                sb.AppendLine($"            component.{field.Name} = new{field.Name};");
            }
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine($"        public {worldName}Entity Replace{componentApiName}({parameters})");
            sb.AppendLine("        {");
            sb.AppendLine($"            if ({poolName}.Has(this))");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Change{componentApiName}({string.Join(", ", component.Fields.Select(f => $"new{f.Name}"))});");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine($"                return Add{componentApiName}({string.Join(", ", component.Fields.Select(f => $"new{f.Name}"))});");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Change{componentApiName}({parameters})");
            sb.AppendLine("        {");
            sb.AppendLine($"            ref {componentFullName} component = ref {poolName}.Get(this);");
            foreach (var field in component.Fields)
            {
                sb.AppendLine($"            component.{field.Name} = new{field.Name};");
            }
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine($"        public {worldName}Entity Remove{componentApiName}()");
            sb.AppendLine("        {");
            sb.AppendLine($"            {poolName}.Remove(this);");
            sb.AppendLine("            return this;");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{worldName}Entity_{component.ComponentName}.g.cs", sb.ToString());
    }
    
    private class StructInfo
    {
        public string[] WorldNames { get; set; } = Array.Empty<string>();
        public string ComponentName { get; set; } = "";
        public string FullNamespace { get; set; } = "";
        public ComponentField[] Fields { get; set; } = Array.Empty<ComponentField>();
    }

    private class ComponentField
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }
}