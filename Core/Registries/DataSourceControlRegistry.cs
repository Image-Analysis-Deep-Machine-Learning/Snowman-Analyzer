using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Snowman.Controls;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Core.Scripting.UserInterface.Controls;

namespace Snowman.Core.Registries;

public static class DataSourceControlRegistry
{
    static DataSourceControlRegistry()
    {
        RegisterPortFactories();
        RegisterVariableFactories();
    }

    private static readonly Dictionary<Type, Func<IDataSource, Control>> DataSourceFactoryRegistry = [];
    
    public static void RegisterDataSourceControlFactory<T>(Func<T, Control> factory) where T : IDataSource
    {
        // wrapper to avoid dynamic invocation every time
        DataSourceFactoryRegistry[typeof(T)] = dataSource => factory((T)dataSource);
    }
    
    public static Control CreateControl(IDataSource dataSource)
    {
        return DataSourceFactoryRegistry.TryGetValue(dataSource.GetType(), out var factory) ? factory(dataSource) : throw new ArgumentException($"No factory registered for {dataSource.GetType().Name}");
    }

    private static void RegisterPortFactories()
    {
        RegisterDataSourceControlFactory<Input>(input =>
        {
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("*"),
                Margin = new Thickness(-10, 0)
            };

            var label = new TextBlock
            {
                Margin = new Thickness(20, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = input.FriendlyName
            };

            var nodePort = new NodePort
            {
                Port = input,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(-4)
            };
        
            grid.Children.Add(label);
            grid.Children.Add(nodePort);

            return grid;
        });
        
        RegisterDataSourceControlFactory<Output>(output =>
        {
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("*"),
                Margin = new Thickness(-10, 0)
            };

            var label = new Label
            {
                Margin = new Thickness(20, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = output.FriendlyName
            };

            var nodePort = new NodePort
            {
                Port = output,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(-4)
            };
        
            grid.Children.Add(label);
            grid.Children.Add(nodePort);

            return grid;
        });
    }
    
    private static void RegisterVariableFactories()
    {
        RegisterDataSourceControlFactory<NumberVariable>(numberVariable => new NumberVariableControl(numberVariable));
        RegisterDataSourceControlFactory<EntitySelector>(entitySelector => new EntitySelectorControl(entitySelector));
        RegisterDataSourceControlFactory<DatasetSelector>(datasetSelector => new DatasetSelectorControl(datasetSelector));
    }
}
