namespace GraphQlClientGenerator.Cc;

internal class Program
{
    // Must be downloaded from Visma in one way or another.
    const string InputPath = @"E:\VismaGqlSchema.json";

    const string GeneratedAssemblyName = "Statler.Visma.Nxt.GraphQlModel.Slim";
    const string OutputDirectory = @$"E:\{GeneratedAssemblyName}";

    static void Main(string[] args)
    {
        var schema = GraphQlGenerator.DeserializeGraphQlSchema(File.ReadAllText(InputPath));
        var config = new GraphQlGeneratorConfiguration()
        {
            TargetNamespace = GeneratedAssemblyName,
            CSharpVersion = CSharpVersion.Newest,
            CodeDocumentationType = CodeDocumentationType.DescriptionAttribute,
            TreatUnknownObjectAsScalar = true,
            FileScopedNamespaces = true,
        };
        var generator = new GraphQlGenerator(config);
        var projectfi = new FileInfo(Path.Combine(OutputDirectory, $"{GeneratedAssemblyName}.csproj"));
        var emitter = new FileSystemEmitter(projectfi.DirectoryName);
        var context = new MultipleFileGenerationContext(schema, emitter, projectfi.Name, GeneratedObjectType.BaseClasses | GeneratedObjectType.DataClasses)
        {
            LogMessage = Console.WriteLine,
        };
        generator.Generate(context);
    }
}
