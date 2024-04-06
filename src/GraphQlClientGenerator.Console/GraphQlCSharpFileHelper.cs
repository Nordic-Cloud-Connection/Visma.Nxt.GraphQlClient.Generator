﻿using System.CommandLine;
using System.CommandLine.IO;

namespace GraphQlClientGenerator.Console;

internal static class GraphQlCSharpFileHelper
{
    public static async Task<int> GenerateGraphQlClientSourceCode(IConsole console, ProgramOptions options)
    {
        try
        {
            var generatedFiles = new List<CodeFileInfo>();
            await GenerateClientSourceCode(console, options, generatedFiles);

            foreach (var fileInfo in generatedFiles)
                console.Out.WriteLine($"File {fileInfo.FileName} generated successfully ({fileInfo.Length:N0} B). ");

            return 0;
        }
        catch (Exception exception)
        {
            console.Error.WriteLine($"An error occurred: {exception}");
            return 2;
        }
    }

    private static async Task GenerateClientSourceCode(IConsole console, ProgramOptions options, List<CodeFileInfo> generatedFiles)
    {
        GraphQlSchema schema;

        if (String.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            var schemaJson = await File.ReadAllTextAsync(options.SchemaFileName);
            console.Out.WriteLine($"GraphQL schema file {options.SchemaFileName} loaded ({schemaJson.Length:N0} B). ");
            schema = GraphQlGenerator.DeserializeGraphQlSchema(schemaJson);
        }
        else
        {
            if (!KeyValueParameterParser.TryGetCustomHeaders(options.Header, out var headers, out var headerParsingErrorMessage))
                throw new InvalidOperationException(headerParsingErrorMessage);

            schema = await GraphQlGenerator.RetrieveSchema(new HttpMethod(options.HttpMethod), options.ServiceUrl, headers);
            console.Out.WriteLine($"GraphQL Schema retrieved from {options.ServiceUrl}. ");
        }
            
        var generatorConfiguration =
            new GraphQlGeneratorConfiguration
            {
                CSharpVersion = options.CSharpVersion,
                ClassPrefix = options.ClassPrefix,
                ClassSuffix = options.ClassSuffix,
                CodeDocumentationType = options.CodeDocumentationType,
                GeneratePartialClasses = options.PartialClasses,
                MemberAccessibility = options.MemberAccessibility,
                IdTypeMapping = options.IdTypeMapping,
                FloatTypeMapping = options.FloatTypeMapping,
                IntegerTypeMapping = options.IntegerTypeMapping,
                BooleanTypeMapping = options.BooleanTypeMapping,
                JsonPropertyGeneration = options.JsonPropertyAttribute,
                EnumValueNaming = options.EnumValueNaming,
                DataClassMemberNullability = options.DataClassMemberNullability,
                IncludeDeprecatedFields = options.IncludeDeprecatedFields,
                FileScopedNamespaces = options.FileScopedNamespaces
            };

        if (!KeyValueParameterParser.TryGetCustomClassMapping(options.ClassMapping, out var customMapping, out var customMappingParsingErrorMessage))
            throw new InvalidOperationException(customMappingParsingErrorMessage);

        foreach (var kvp in customMapping)
            generatorConfiguration.CustomClassNameMapping.Add(kvp);

        if (!String.IsNullOrEmpty(options.RegexScalarFieldTypeMappingConfigurationFile))
            generatorConfiguration.ScalarFieldTypeMappingProvider =
                new RegexScalarFieldTypeMappingProvider(
                    RegexScalarFieldTypeMappingProvider.ParseRulesFromJson(await File.ReadAllTextAsync(options.RegexScalarFieldTypeMappingConfigurationFile)));

        var generator = new GraphQlGenerator(generatorConfiguration);

        if (options.OutputType is OutputType.SingleFile)
        {
            await File.WriteAllTextAsync(options.OutputPath, generator.GenerateFullClientCSharpFile(schema, options.Namespace, console.Out.WriteLine));
            generatedFiles.Add(new CodeFileInfo { FileName = options.OutputPath, Length = new FileInfo(options.OutputPath).Length });
        }
        else
        {
            var projectFileInfo =
                options.OutputPath is not null && options.OutputPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                    ? new FileInfo(options.OutputPath)
                    : null;

            var codeFileEmitter = new FileSystemEmitter(projectFileInfo?.DirectoryName ?? options.OutputPath);
            var multipleFileGenerationContext = new MultipleFileGenerationContext(schema, codeFileEmitter, options.Namespace, projectFileInfo?.Name) { LogMessage = console.Out.WriteLine };
            generator.Generate(multipleFileGenerationContext);
            generatedFiles.AddRange(multipleFileGenerationContext.Files);
        }
    }
}