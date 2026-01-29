using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Snowman.Controls;

namespace Snowman.Core.Scripting.DataSource;

/// <summary>
/// Registry for factories which construct controls for variables and ports
/// </summary>
public class DataSourceControlFactory
{
    static DataSourceControlFactory()
    {
        RegisterPortFactories();
    }

    private static readonly Dictionary<Type, Func<IDataSource, Control>> FactoryRegistry = [];
    
    public static void RegisterDataSourceControlFactory<T>(Func<T, Control> factory) where T : IDataSource
    {
        // wrapper to avoid dynamic invocation every time
        FactoryRegistry[typeof(T)] = dataSource => factory((T)dataSource);
    }
    
    public static Control CreateControl(IDataSource dataSource)
    {
        return FactoryRegistry.TryGetValue(dataSource.GetType(), out var factory) ? factory(dataSource) : throw new ArgumentException($"No factory registered for {dataSource.GetType().Name}");
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
}
