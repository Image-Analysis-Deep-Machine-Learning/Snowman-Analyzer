using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Core.Services;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Data;

public static class NodeGraphDataConverter
{
    private static readonly Func<object?, string> ToStr = x => x?.ToString() ?? string.Empty;
    private static readonly Func<string?, string> FromStr = x => x ?? string.Empty;
    private static readonly Dictionary<Type, Func<Variable, VariableData>> VariableSerializers = [];
    private static readonly Dictionary<Type, Action<Variable, VariableData, IServiceProvider>> VariableDeserializers = [];
    
    static NodeGraphDataConverter()
    {
        RegisterVariableSerializers();
        RegisterVariableDeserializers();
    }

    public static VariableData Serialize(Variable variable)
    {
        return VariableSerializers[variable.GetType()](variable);
    }

    public static void Deserialize(Variable variable, VariableData variableData, IServiceProvider serviceProvider)
    {
        VariableDeserializers[variable.GetType()](variable, variableData, serviceProvider);
    }
    
    private static void RegisterVariableSerializer<TVariable>(Action<TVariable, (VariableData Data, XmlDocument Document)> serializer) where TVariable : Variable
    {
        VariableSerializers[typeof(TVariable)] = variable =>
        {
            var vData = CreateVariableData(variable.Name);
            serializer((TVariable)variable, vData);
            return vData.Item1;
        };
    }
    
    private static void RegisterVariableDeserializer<TVariable>(Action<TVariable, VariableData, IServiceProvider> serializer) where TVariable : Variable
    {
        VariableDeserializers[typeof(TVariable)] = (variable, variableData, serviceProvider) => serializer((TVariable)variable, variableData, serviceProvider);
    }
    
    private static void RegisterVariableSerializers()
    {
        RegisterVariableSerializer<DatasetSelector>((v, vData) =>
        {
            v.CustomDatasetPath.ToParsedData(vData, ToStr);
            v.IsCustomPathSelected.ToParsedData(vData, ToStr);
        });
        RegisterVariableSerializer<EntitySelector>((v, vData) =>
        {
            v.TypedValue.ToParsedData(vData, x => ToStr(x?.Id ?? -1));
        });
        RegisterVariableSerializer<NumberVariable>((v, vData) =>
        {
            v.TypedValue.ToParsedData(vData, x => x.ToString(CultureInfo.InvariantCulture));
        });
    }

    private static void RegisterVariableDeserializers()
    {
        RegisterVariableDeserializer<DatasetSelector>((v, vData, _) =>
        {
            v.CustomDatasetPath.LoadFromParsedData(v, vData, FromStr);
            v.IsCustomPathSelected.LoadFromParsedData(v, vData, x => bool.TryParse(x, out var result) && result);
        });
        RegisterVariableDeserializer<EntitySelector>((v, vData, serviceProvider) =>
        {
            v.TypedValue.LoadFromParsedData(v, vData, x => int.TryParse(x, out var selectedEntityId) ? serviceProvider.GetService<IEntityManager>().GetEntityById(selectedEntityId) : null);
        });
        RegisterVariableDeserializer<NumberVariable>((v, vData, _) =>
        {
            v.TypedValue.LoadFromParsedData(v, vData, x => decimal.TryParse(x, CultureInfo.InvariantCulture, out var result) ? result : 0);
        });
    }

    private static (VariableData, XmlDocument) CreateVariableData(string variableName)
    {
        var documentFactory = new XmlDocument();
        var properties = documentFactory.CreateElement("Properties");
        return (new VariableData { Name = variableName, Properties = properties },  documentFactory);
    }

    // DO NOT CHANGE TO EXTENSION BLOCK (extension<T>(T property) {...})
    // CallerArgumentExpression attribute does not work when property is in extension block
    private static void ToParsedData<T>(this T property, (VariableData Data, XmlDocument Document) vData, Func<T?, string> converter, [CallerArgumentExpression(nameof(property))] string? propertyExpression = null)
    {
        var lastIndexOfDot = propertyExpression?.LastIndexOf('.') ?? throw new Exception($"Property expression from {property} cannot be null.");
        
        if (lastIndexOfDot < 0) throw new Exception($"Property expression from {property} cannot must be in the following format: xxx.Property and {propertyExpression} does not satisfy this requirement.");
        
        var propertyName = propertyExpression[(lastIndexOfDot + 1)..];
        var element = vData.Document.CreateElement(propertyName);
        
        element.InnerText = converter(property);
        vData.Data.Properties.AppendChild(element);
    }

    private static void LoadFromParsedData<T, TVariable>(this T property, TVariable variable, VariableData variableData, Func<string?, T> converter, [CallerArgumentExpression(nameof(property))] string? propertyExpression = null) where TVariable : Variable
    {
        var lastIndexOfDot = propertyExpression?.LastIndexOf('.') ?? throw new Exception($"Property expression from {property} cannot be null.");
        
        if (lastIndexOfDot < 0) throw new Exception($"Property expression from {property} cannot must be in the following format: xxx.Property and {propertyExpression} does not satisfy this requirement.");
        
        var propertyName = propertyExpression[(lastIndexOfDot + 1)..];
        var propertyValue = variableData.Properties[propertyName]?.InnerText;
        
        variable.GetType().GetProperty(propertyName)?.SetValue(variable, converter(propertyValue));
    }
}
