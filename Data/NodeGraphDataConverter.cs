using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using Snowman.Core.Entities;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;

namespace Snowman.Data;

public static class NodeGraphDataConverter
{
    private static readonly Func<object?, string> ToStr = x => x?.ToString() ?? string.Empty;
    private static readonly Dictionary<Type, Func<Variable, VariableData>> VariableSerializers = [];
    private static readonly Dictionary<Type, Action<Variable, VariableData>> VariableDeserializers = [];
    
    static NodeGraphDataConverter()
    {
        RegisterVariableSerializers();
        RegisterVariableDeserializers();
    }
    
    private static void RegisterVariableSerializer<TVariable>(Func<TVariable, VariableData> serializer) where TVariable : Variable
    {
        VariableSerializers[typeof(TVariable)] = variable => serializer((TVariable)variable);
    }
    
    private static void RegisterVariableDeserializer<TVariable>(Action<TVariable, VariableData> serializer) where TVariable : Variable
    {
        VariableDeserializers[typeof(TVariable)] = (variable, variableData) => serializer((TVariable)variable, variableData);
    }
    
    private static void RegisterVariableSerializers()
    {
        RegisterVariableSerializer<DatasetSelector>(v =>
        {
            var root = CreateRoot();
            v.CustomDatasetPath.ToParsedData(root, ToStr);
            v.IsCustomPathSelected.ToParsedData(root, ToStr);

            return new VariableData { Name = v.Name, Properties = root };
        });
        RegisterVariableSerializer<EntitySelector>(v =>
        {
            var root = CreateRoot();
            v.TypedValue.ToParsedData(root, x => ToStr(x?.Id ?? -1));
            
            return new VariableData { Name = v.Name, Properties = root };
        });
        RegisterVariableSerializer<NumberVariable>(v =>
        {
            var root = CreateRoot();
            v.TypedValue.ToParsedData(root, x => x.ToString(CultureInfo.InvariantCulture));

            return new  VariableData { Name = v.Name, Properties = root };
        });
    }

    private static void RegisterVariableDeserializers()
    {
        RegisterVariableDeserializer<DatasetSelector>((v, vData) =>
        {
            
        });
        RegisterVariableDeserializer<EntitySelector>((v, vData) =>
        {
            
        });
        RegisterVariableDeserializer<NumberVariable>((v, vData) =>
        {
            
        });
    }

    private static XmlElement CreateRoot()
    {
        var dummyFactory = new XmlDocument();
        return dummyFactory.CreateElement("Properties");
    }

    private static void ToParsedData<T>(this T obj, in XmlElement root, Func<T?, string> converter, [CallerArgumentExpression(nameof(obj))] string? propertyExpression = null)
    {
        var lastIndexOfDot = propertyExpression?.LastIndexOf('.') ?? throw new Exception($"Property expression from {obj} cannot be null.");
        
        if (lastIndexOfDot < 0) throw new Exception($"Property expression from {obj} cannot must be in the following format: xxx.Property and {propertyExpression} does not satisfy this requirement.");
        
        var propertyName = propertyExpression[lastIndexOfDot..];
        var dummyFactory = new XmlDocument();
        var element = dummyFactory.CreateElement(propertyName);
        
        element.InnerText = converter(obj);
        root.AppendChild(element);
    }
}
