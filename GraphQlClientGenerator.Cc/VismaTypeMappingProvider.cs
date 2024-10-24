using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQlClientGenerator.Cc;
internal class VismaTypeMappingProvider : IScalarFieldTypeMappingProvider
{
    private readonly List<GraphQlTypeKind> kinds = new();

    public ScalarFieldTypeDescription GetCustomScalarFieldType
        (
        GraphQlGeneratorConfiguration configuration,
        GraphQlType baseType,
        GraphQlTypeBase valueType,
        string valueName
        )
    {
        valueType = (valueType as GraphQlFieldType)?.UnwrapIfNonNull() ?? valueType;

        if (valueType.Kind == GraphQlTypeKind.Enum)
            return new ScalarFieldTypeDescription { NetTypeName =
                $"{configuration.ClassPrefix}{NamingHelper.ToPascalCase(valueType.Name)}{configuration.ClassSuffix}?"
            };

        if (valueType.Kind != GraphQlTypeKind.Scalar)
        {
            if (!kinds.Contains(valueType.Kind))
            {
                Debug.WriteLine($"Ignored value type {valueType.Kind}/{valueType.Name}");
                kinds.Add(valueType.Kind);
            }
            return DotNetName(configuration, "object?");
        }

        return valueType.Name switch
        {
            "String" => DotNetName(configuration, "string?"),
            "Byte" => DotNetName(configuration, "byte"),
            "Short" => DotNetName(configuration, "short"),
            "Long" => DotNetName(configuration, "long"),
            "Decimal" => DotNetName(configuration, "decimal"),
            "Date" => DotNetName(configuration, "DateOnly"),
            "DateTime" => DotNetName(configuration, "DateTime"),
            "TimeOnly" => DotNetName(configuration, "TimeOnly"),
            _ => throw new NotSupportedException($"Unsupported value type {valueType.Name}")
        };
    }

    private static ScalarFieldTypeDescription DotNetName(GraphQlGeneratorConfiguration config, string name) =>
        new()
        {
            NetTypeName = name + (config.DataClassMemberNullability == DataClassMemberNullability.AlwaysNullable
            ? "?" : string.Empty),
        };
}
