# Code documentation

Table of contents:

- [Services](#services)
- [How to categorize code and classes](#how-to-categorize-code-and-classes)
- [Node Graph](#node-graph)

There is also a [UML Diagram](Snowman%20UML.svg) which shows a prototype architecture of Snowman App.

## Services
Snowman uses Dependency Injection design pattern to provide access to services across the project. This step is necessary to avoid tight coupling of objects. ServiceProvider instance is specific to the MainWindow to allow multiple open projects in the future if needed. ServiceProvider uses IServiceProvider interface with which the developer interacts. To avoid wrong suggestions for this interface (as an interface with same name is in the System namespace as well) add `System.IServiceProvider` into exclusion list for importing in Settings -> Editor -> General -> -> Auto Import -> Filtering -> 'Exclude the following types/members from import features' (or use Ctrl + Alt + 0 + S key bind). This is for the Rider IDE which is suggested over Visual Studio due to better AvaloniaUI Designer and Intellisense support. 

Developers should create services for logic used by more than one class. Services always consist of an interface and an implementation. Implementation is created and registered in ServiceProvider as soon as possible, preferably in the ServiceProvider implementation constructor. Only the interface is used in other parts of the application.

### EventManager
EventManager service provides an easy-to-use and flexible method of allowing any class with a ServiceProvider instance to register itself as an event supplier (an equivalent of an Observable interface). Any class can also register an action on any event supplier. Event suppliers are always denoted by an interface (that extends `IEventSupplier` interface). This interface contains only the events it "supplies" and on which the actions registered via EventManager can register their handlers.

An example is [IEntityEventSupplier.cs](../Events/Suppliers/IEntityEventSupplier.cs) that is implemented by the [EntityManager service implementation](../Core/Services/Impl/EntityManagerImpl.cs). Entity manager invokes the events and any class can register an action on this supplier.

The supplier (e.g. IEntityEventSupplier) cannot be returned by the EntityManager directly to access its events. Rather you can register an action (lambda) that gets the supplier in its parameter, then register the handler in the lambda as seen in [EntitySelector.cs](../Core/Scripting/DataSource/Variables/EntitySelector.cs)  in the `Copy()` method. This is done to support an out-of-order event registration - the ability to register an action on a supplier that is not yet registered in the EventManager. This is a step above regular Observer design pattern which usually works by an Observer registering itself into an Observable - which must exist to call its instance method.

The EventManager service has the ability to register multiple suppliers of the same type (using the same interface type) and execute all registered actions on all of them. This comes at the cost of being unable to selectively register an action on only some suppliers of a certain type.

The EventManager should not be misused to execute code in an arbitrary class from a different arbitrary class. Events should always serve as notifications for other objects to react to. Single use events are discouraged, though it is worth noting that some events can have only one handler and still be correct because they can have other observers in the future.

## How to categorize code and classes

The rules are not strict, but sticking to them makes it easier for others to work on the project. The application is logically divided into two parts - **GUI** and **logic**. GUI is made up of **controls**. There are built-in controls from AvaloniaUI, imported controls from 3rd party libraries (such as planned Dock.Avalonia package) and custom controls made by the developers of Snowman.

### How to make controls

First some explanation of used terms:
 - **DataContext** - container for data required for the control. It has properties referenced by the XAML designer file used to bind data.
 - **Data binding** - developers can use data bindings in XAML files to reference properties in the DataContext of the control. These can be simple properties or custom containers (e.g. crates) with a DataTemplate used to render a custom hierarchy of controls. Examples of both can be found in [FrameTimeline](../Controls/FrameTimeline.axaml) control which accesses property **Frames** from its DataContext, which is a custom iterable container of TimelineFrame objects. These objects have properties like Label, Image or Invisible which are then used in the DataTemplate.

Each custom control **SHOULD** extend the [UserControlWrapper](../Controls/UserControlWrapper.cs) class. This generic wrapper provides a typed (cast) access to the DataContext property so the developer does not need to cast the DataContext each time they use it in their custom control. It also ensures correct assignment of said DataContext. This is not required and if the control is not compatible with this approach (e.g. more than one DataContext class type used, nonavoidable extending of different base class etc.) it can be skipped.

Constructor **CANNOT** contain any logic or code that requires the DataContext and must be parameterless. DataContext cannot be created in the constructor, use [DataContextInjector](../DataContexts/DataContextInjector.cs) to create correct DataContext for a control and do other setup you would usually do in a constructor. If the custom control is to be added dynamically via code and never referenced in XAML, parametrized constructor with DataContext set in them can be used.

Previously used `OnAttachedToLogicalTree` has been replaced with the aforementioned DataContextInjector.

Control DataContext **SHOULD NOT** know anything about the control class (.axaml.cs). Do not pass a reference to the control class to the DataContext. This disconnect creates several challenges:
1. Control reacting to changes to data in DataContext - use events instead. Create an event describing the change in DataContext and add a handler to it after creation of DataContext in the control class. [Example](../Controls/FrameTimeline.axaml.cs)
2. DataContext needs properties of the control to initialize/work properly, e.g. Bounds. For this purpose a `OneWayToSource` binding can be used. This allows to "mirror" value of a property in the control class into DataContext (including changes to it) without tight coupling of these two classes. See [FrameTimeline XAML file](../Controls/FrameTimeline.axaml) for an example.

Try to make a custom control for anything that can be encapsulated with its own logic or any group/hierarchy of controls that can be reused elsewhere.

DataContexts that are set via DataContextInjector should be divided into two parts (use partial classes). Main part in the [DataContexts](../DataContexts) folder and additional boilerplate in [DummyDataContexts](../Designer/DummyDataContexts.cs) class. Keep them alphabetically ordered if possible. These partial classes will have the parameterless constructor required by UserControlWrapper generic parameter `T : new()` constraint as well as default values for all readonly fields and properties. This is required for AvaloniaUI to not shit itself. Other dummy objects used to avoid designer errors and warnings as well as other issues are in the [Designer](../Designer) folder

#### Steps to create a new custom control

1. Add new Avalonia User Control through Rider IDE menu (right click on Controls folder → Add). Pick a name.
2. Create new public partial DataContext class in Snowman.DataContexts namespace named \[control_name\]DataContext.
3. Due to technical limitations of current design, a parameterless constructor must be available. The partial class with this constructor should be in the [DummyDataContexts](../Designer/DummyDataContexts.cs) file.
4. Edit the .axaml.cs file for the control to extend from UserControlWrapper instead of UserControl. The generic type must be the \[control_name\]DataContext class.
5. Edit the .axaml file to fit the template below. Replace \[control_name\] with the name of the control. See [Viewport XAML file](../Controls/Viewport.axaml) for an example:

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
</ctrl:UserControlWrapper>
```

### How to add new entities

Each entity is in [Entities](../Core/Entities) folder. Entities can be composited together. The approach of adding and managing child entities is up to the developer.

Entity has a Render method where the entity is being rendered using DrawingContext from the viewport. Always check if the `IsVisible` property is true before drawing the entity (same for hit evaluation method). Children are not automatically rendered and must be rendered separately. Same applies for the label with ID. This will change in the future to have less duplicate code with the power of template methods.

Each entity needs its Tool in [Tools](../Core/Tools) folder. This tool should extend the EntityEditTool and override required functionality. This will change in the future to use commands instead of infinite branching of overriden code.

The tool needs to be registered in the [ToolRegistry](../Core/Tools/ToolRegistry.cs) class.

Entities must have corresponding Data classes in [ProjectData](../Data/ProjectData.cs) as well as converter factories in [ProjectDataConverter](../Data/ProjectDataConverter.cs) class. The EntityData class in [ProjectData.cs](../Data/ProjectData.cs) must have an XmlInclude attribute for each entity type to tell the XML parser what classes it can encounter.

### How to add new variables

Main variable classes are in [Variables](../Core/Scripting/DataSource/Variables) folder. These classes always extend the [GenericVariableWrapper](../Core/Scripting/DataSource/GenericVariableWrapper.cs) class to add property change support for GUI as well as strongly typed access to the Value property. Generic parameter of the class should be the type stored in the TypedValue property. This class acts as a DataContext for the GUI controls so it **MUST** have a parameterless constructor. Preferably in [DummyVariables](../Designer/DummyVariables.cs) class as a part of a partial class. This should include adding any other dummy objects needed for designer to load.

Variables have controls in [UserInterface/Controls](../Core/Scripting/UserInterface/Controls) folder. These controls extend VariableControl as a base class to add additional constraints to the DataContext for generics safety.

It is important to register the Variables as well as their controls (prototypes/factories) into [VariablePrototypeRegistry](../Core/Scripting/DataSource/Variables/VariablePrototypeRegistry.cs) and [DataSourceControlRegistry](../Core/Scripting/UserInterface/DataSourceControlRegistry.cs) respectively. This allows the ScriptBuilder to create a ScriptNode when parsing the .script file and NodeControlBuilder to build the node. This registration can technically be done automatically using reflection during runtime (to find all existing subclasses of the Variable and VariableControl types), so it might change in the future to automatically register any new types.

### General suggestions

Order and group class members as close to this list as possible:
1. constants
2. fields
3. properties
4. constructors
5. methods
6. nested classes

Constants, fields and constructors should be ordered by visibility from private to public (e.g. first private, then protected, then public, never make public fields, use properties instead). Properties and methods should be ordered from public to private.

Try to add more complex (3+ lines of code) event handlers as their own methods instead of lambdas if possible. Always private.

Always have one empty new line at the end of each file. This helps with git diffs.

## Node graph

Node graph is partially implemented now.

### OutputNodes

Abstract class `OutputNode` encapsulates a node which acts as the final destination of data from which the data are processed and send into the application. Currently implemented OutpuNodes are: [LoggerOutputNode](../Core/Scripting/Nodes/OutputNodes/LoggerOutputNode.cs)

An example of such node is EventTimelineOutputNode. This node aggregates data (currently in unknown format) into something the EventTimeline can process, basically acting as a middleman. The graph can have multiple EventTimelineOutputNodes each aggregating different data, but always creating output in the same format. This node can have arbitrary options needed to create data for the EventTimeline. This may include:

 - Event name to distinguish different events
 - Event group to allow grouping of multiple events into one timeline (this can alternatively be done by the user in the timeline itself to allow more flexibility)
 - any other options/settings needed for the timeline to correctly render