# Code documentation

Table of contents:

- [Services](#services)
- [How to categorize code and classes](#how-to-categorize-code-and-classes)
- [Node Graph](#node-graph)

There is also a [UML Diagram](Snowman%20UML.svg) which shows prototype architecture of Snowman.

## Services
Snowman uses Dependency Injection design pattern to provide access to services across the project. This step is necessary to avoid tight coupling of objects. ServiceProvider instance is specific to the MainWindow to allow multiple open projects in the future if needed. ServiceProvider uses IServiceProvider interface with which the developer interacts. To avoid wrong suggestions for this interface (as it is in the System namespace as well) add `System.IServiceProvider` into exclusion list for importing in Settings -> Editor -> General -> -> Auto Import -> Filtering -> 'Exclude the following types/members from import features' (or Ctrl + Alt + 0 + S key bind)

Developers should create services for logic used by more than one object. Services always consist of an interface and an implementation. Implementation is created and registered in ServiceProvider as soon as possible, preferably in the ServiceProvider implementation constructor. Only the interface is used in other parts of the application, even though it contains an instance of the implementation.

### EventManager
...

## How to categorize code and classes

The rules are not strict, but sticking to them makes it easier for others to work on the project. The application is logically divided into two parts - **GUI** and **logic**. GUI is made up of **controls**. There are built-in controls from AvaloniaUI, imported controls from 3rd party libraries (such as planned Dock.Avalonia package) and custom controls made by the developers of Snowman.


### How to make controls

First some explanation of used terms:
 - **DataContext** - container for data required for the control. It has properties referenced by the XAML designer file used to bind data.
 - **Data binding** - developers can use data bindings in XAML files to reference properties in the DataContext of the control. These can be simple properties or custom containers (eg. crates) with a datatemplate used to render a custom hierarchy of controls. Examples of both can be found in [FrameTimeline](../Controls/FrameTimeline.axaml) control which accesses property **Frames** from its DataContext, which is a custom iterable container of TimelineFrame objects. These objects have properties like Label, Image or Invisible which are then used in the DataTemplate.

Each custom control **SHOULD** extend the [UserControlWrapper](../Controls/UserControlWrapper.cs) class. This generic wrapper provides easy cast access to the DataContext property so the developer does not need to cast the DataContext each time they use it in their custom control. This is not required and if the control is not compatible with this approach (eg. more than one DataContext class, nonavoidable extending of different base class etc.) it can be skipped.

Constructor **CANNOT** contain any logic or code that requires the DataContext and DataContext cannot be created in the constructor either.

Overriden method `OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)` **MUST** be used to create DataContext programatically. ServiceProviderAttachedProperty is available in this method and can be used to retrieve the ServiceProvider. See [ToolBar class](../Controls/ToolBar.axaml.cs) for a simple example. This method must call its base overriden method from parent class on the last line to prevent issues with event propagation (this line of code is automatically inserted by the IDE). The developer is free to use the ServiceProvider for anything, however avoid putting data directly used by UI in the control class as that's the DataContext's responsibility.

Control DataContext **SHOULD NOT** know anything about the control class (.axaml.cs). Do not pass a reference to the control class to the DataContext. This disconnect creates several challenges:
1. Control reacting to changes to data in DataContext - use events instead. Create an event describing the change in DataContext and add a handler to it after creation of DataContext in the control class. [Example](../Controls/FrameTimeline.axaml.cs)
2. DataContext needs properties of the control to initialize/work properly, eg. Bounds. For this purpose a `OneWayToSource` binding can be used. This allows to "mirror" value of a property in the control class into DataContext (including changes to it) without tight coupling of these two classes. See [FrameTimeline XAML file](../Controls/FrameTimeline.axaml) for an example.

Try to make a custom control for anything that can be encapsulated with its own logic or any group/hierarchy of controls that can be reused elsewhere.

To avoid null reference warnings in DataContext for fields (due to parameterless constructor not initializing anything, see steps below), initialize these fields with `null!` literal in declaration. This approach can be used with readonly fields as well.

#### Steps to create a new custom control

1. Add new Avalonia User Control through Rider IDE menu (right click on Controls folder -> Add). Pick a name.
2. Create new DataContext class in Snowman.Datacontexts namespace named \[control_name\]DataContext.
3. Due to technical limitations of current design, an empty parameterless constructor must be available. The simplest solution is to add a [primary constructor](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors). If another constructor is needed, for example with a ServiceProvider instance passed in, it must call the empty constructor. See [ToolBarDataContext class](../DataContexts/ToolBarDataContext.cs) for a simple example.
4. Edit the .axaml.cs file for the control to extend from UserControlWrapper instead of UserControl. The generic type must be the \[control_name\]DataContext class.
5. Edit the .axaml file to fit the template below. Replace \[control_name\] with the name of the control. The DataContext line sets the DataContext to the correct type (and an empty instance - hence the required parameterless constructor) and is required to avoid retarded exceptions from our beloved AvaloniaUI framework. See [Viewport XAML file](../Controls/Viewport.axaml) for an example:

```html
<ctrl:UserControlWrapper x:TypeArguments="dataContext:[control_name]DataContext"
                             xmlns="https://github.com/avaloniaui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
                             x:Class="Snowman.Controls.[control_name]"
                             xmlns:dataContext="using:Snowman.DataContexts"
                             xmlns:ctrl="using:Snowman.Controls"
                             x:DataType="dataContext:[control_name]Context">
    <ctrl:UserControlWrapper.DataContext><dataContext:[control_name]DataContext></dataContext:[control_name]DataContext></ctrl:UserControlWrapper.DataContext>
</ctrl:UserControlWrapper>
```

### General suggestions

Order and group class members as close to this list as possible:
1. constants
2. fields
3. properties
4. constructors
5. methods

Constants, fields and constructors should be ordered by visibility from private to public (eg. first private, then protected, then public, never make public fields, use properties instead). Properties and methods should be ordered from public to private.

Try to add more complex (3+ lines of code) event handlers as their own methods instead of lambdas if possible. Always private and at the end of class file.

Always have one empty new line at the end of each file. This helps with git diffs.

## Node graph

Node graph is not yet implemented

### OutputNodes

Abstract class `OutputNode` encapsulates a node which acts as the final destination of data from which the data are processed and send into the application.

An example of such node is EventTimelineOutputNode. This node aggregates data (currently in unknown format) into something the EventTimeline can process, basically acting as a middleman. The graph can have multiple EventTimelineOutputNodes each aggregating different data, but always creating output in the same format. This node can have arbitrary options needed to create data for the EventTimeline. This may include:

 - Event name to distinguish different events
 - Event group to allow grouping of multiple events into one timeline (this can alternatively be done by the user in the timeline itself to allow more flexibility)
 - any other options/settings needed for the timeline to correctly render