using Avalonia.Controls;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;

namespace Snowman;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // the most important lines of this application that handle the dependency injection magic
        var serviceProvider = new ServiceProviderImpl();
        serviceProvider.RegisterService<IStorageProviderService>(new StorageProviderServiceImpl(StorageProvider));
        DataContext = new MainWindowDataContext(serviceProvider);
        ServiceProviderAttachedProperty.SetProvider(this, serviceProvider);

        _ = new SnowmanApp(serviceProvider);

        InitializeComponent();
        
        serviceProvider.RegisterService<ILoggerService>(new LoggerServiceImpl(LoggerTextBox));
    }
}
