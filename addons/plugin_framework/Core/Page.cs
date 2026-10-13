#if TOOLS
using System;
using Godot;

namespace Addons.PluginFramework.Core;

/// <summary>
/// Abstract base class for all plugin pages in the system.
/// Provides lifecycle management, strongly-typed data handling (<c>TData</c>), a strongly-typed plugin (<c>TPlugin</c>),
/// and integration with <c>PageDock</c>.
/// </summary>
/// <remarks>
/// <para>Pages are automatically initialized when added to a <c>PageDock</c>.</para>
/// <para>Override <c>Initialize</c> to set up your page's UI and logic.</para>
/// <para>Access the plugin instance via the <c>plugin</c> field.</para>
/// <example>
/// Example Page:
/// <code>
/// public struct MyPageData{
///     public int Counter;
/// }
/// 
/// [Tool]
/// public class MyPage : Page&lt;MyPageData, MyPlugin&gt;
/// {
///     [Export] private Label label;
///     public override void Initialize()
///     {
///         // Setup your page here
///         label.Text = $"Count: {data.Counter}";
///         Title = $"Refreshes: {data.Counter}";
///     }
/// 
///     // Add as Signal to Button
///     public void Increment()
///     {
///         data.Counter++;
///         dock.ReloadPage(data);
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public abstract partial class Page<TData, TPlugin> : PageControl
    where TPlugin : FrameworkPlugin
{
    /// <summary>The data handed to the page via <c>PageDock.SwitchPage</c>; <c>default</c> if none was given.</summary>
    protected TData data;

    /// <summary>The <c>PageDock</c> that hosts this page.</summary>
    protected PageDock dock;

    /// <summary>The owning plugin, typed to the concrete plugin class of the addon.</summary>
    protected TPlugin plugin;

    /// <summary>
    /// Initializes the page with its data and sets up the UI and logic.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called automatically by the framework after the page is added to a <c>PageDock</c>
    /// and all required dependencies are validated. Override this method to:
    /// <list type="bullet">
    ///   <item>Set up your page's UI elements (e.g., labels, buttons, containers).</item>
    ///   <item>Bind event handlers (e.g., button clicks, signal connections).</item>
    ///   <item>Initialize your page using the strongly-typed <c>data</c> property.</item>
    ///   <item>Set the <c>Title</c> property to update the dock's display name.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The <c>data</c> property is already assigned when this method is called and contains
    /// the strongly-typed data passed to the page (e.g., via <c>PageDock.SwitchPage</c>).
    /// If no data was provided, <c>data</c> will be <c>default(TData)</c>.
    /// </para>
    /// <para>
    /// Example workflow:
    /// <list type="number">
    ///   <item>Create UI elements (e.g., <c>new Label()</c>).</item>
    ///   <item>Add them to the control hierarchy (<c>AddChild</c>).</item>
    ///   <item>Bind UI elements to data (e.g., <c>label.Text = data.SomeProperty</c>).</item>
    ///   <item>Connect signals (e.g., button <c>pressed</c> to a local method).</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Important:</strong> Avoid long-running operations in this method.
    /// Use <c>CallDeferred</c> for operations that require the scene tree to be ready.
    /// </para>
    /// </remarks>
    public abstract void Initialize();

    /// <summary>
    /// Initializes the <c>Page</c> with <c>data</c>.
    /// </summary>
    /// <remarks>
    /// Used for dynamically spawning content like varying items and setting values.
    /// <para>
    /// This method ensures every bit of types and references is correct by:
    /// </para>
    /// <list type="bullet">
    ///   <item>Validating the editor context to prevent runtime execution</item>
    ///   <item>Checking for a valid parent <c>PageDock</c> instance</item>
    ///   <item>Verifying the plugin reference from the dock</item>
    ///   <item>Performing type-safe data casting to <c>TData</c></item>
    /// </list>
    /// <para>
    /// The <c>Initialize()</c> call is wrapped in a try-catch block to ensure safe execution.
    /// Any exceptions thrown during initialization are caught and logged via
    /// <c>ExceptionHandler.ThrowUnexpectedException</c>.
    /// </para>
    /// Logs via <c>ExceptionHandler.ThrowInvalidPageDataException</c> if the data type is incorrect.
    /// </remarks>
    public override void _Ready()
    {
        // Only run in editor context
        if (!Engine.IsEditorHint())
        {
            return;
        }

        // Only initialize if we're actually part of the plugin dock
        var parent = GetParent();
        if (parent is not PageDock dock)
        {
            // Exception is correct, but throws everytime a Pagebase scene is opened 
            // ExceptionHandler.ThrowMissingDockParentException(GetPath());
            return;
        }

        this.dock = dock;
        plugin = dock.plugin as TPlugin;

        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException($"{GetType().Name} {nameof(_Ready)}");
            return;
        }

        // Handle data
        if (dock.data != null && dock.data is TData typedData)
        {
            data = typedData;
        }
        else if (dock.data != null) // Data exists but wrong type
        {
            ExceptionHandler.ThrowInvalidPageDataException(
                GetType().Name,
                typeof(TData).Name,
                dock.data.GetType().Name
            );
            data = default;
        }
        else
        {
            data = default;
        }

        try
        {
            Initialize();
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"Page.Initialize - {GetType().Name} {nameof(_Ready)}");
        }
    }
}
#endif
