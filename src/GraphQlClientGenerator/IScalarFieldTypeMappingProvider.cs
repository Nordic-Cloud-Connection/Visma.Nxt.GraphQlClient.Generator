﻿namespace GraphQlClientGenerator
{
    public interface IScalarFieldTypeMappingProvider
    {
        ScalarFieldTypeDescription GetCustomScalarFieldType(GraphQlGeneratorConfiguration configuration, GraphQlType baseType, GraphQlTypeBase valueType, string valueName);
    }

    public struct ScalarFieldTypeDescription
    {
        public string NetTypeName { get; set; }
        public string FormatMask { get; set; }
    }

    public sealed class DefaultScalarFieldTypeMappingProvider : IScalarFieldTypeMappingProvider
    {
        public static readonly DefaultScalarFieldTypeMappingProvider Instance = new DefaultScalarFieldTypeMappingProvider();

        private DefaultScalarFieldTypeMappingProvider()
        {
        }

        public ScalarFieldTypeDescription GetCustomScalarFieldType(GraphQlGeneratorConfiguration configuration, GraphQlType baseType, GraphQlTypeBase valueType, string valueName)
        {
            valueName = NamingHelper.ToPascalCase(valueName);

            if (valueName == "From" || valueName == "ValidFrom" || valueName == "To" || valueName == "ValidTo" ||
                valueName == "CreatedAt" || valueName == "UpdatedAt" || valueName == "ModifiedAt" || valueName == "DeletedAt" ||
                valueName.EndsWith("Timestamp"))
                return new ScalarFieldTypeDescription { NetTypeName = "DateTimeOffset?" };

            valueType = (valueType as GraphQlFieldType)?.UnwrapIfNonNull() ?? valueType;
            if (valueType.Kind == GraphQlTypeKind.Enum)
                return new ScalarFieldTypeDescription { NetTypeName = configuration.ClassPrefix + NamingHelper.ToPascalCase(valueType.Name) + configuration.ClassSuffix + "?" };

            var dataType = valueType.Name == GraphQlTypeBase.GraphQlTypeScalarString ? "string" : "object";
            return new ScalarFieldTypeDescription { NetTypeName = GraphQlGenerator.AddQuestionMarkIfNullableReferencesEnabled(configuration, dataType) };
        }
    }
}