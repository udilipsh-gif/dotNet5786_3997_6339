namespace PL;

/// <summary>
/// Defines a contract for windows that support state updates without closing and reopening.
/// </summary>
/// <remarks>
/// This interface enables the reactivation pattern where an existing window can be updated
/// with new parameters instead of creating a new window instance. This is particularly useful
/// for maintaining a single instance of certain windows (like list windows) while allowing
/// their state to be updated when the user attempts to open them again with different parameters.
/// <para><b>Usage Pattern:</b></para>
/// <list type="number">
/// <item>
/// <description>User attempts to open a window that's already open (e.g., OrderListWindow)</description>
/// </item>
/// <item>
/// <description>The <see cref="Tools.OpenOrActivateWindow{T}"/> method detects the existing window</description>
/// </item>
/// <item>
/// <description>Instead of creating a new instance, it calls <see cref="UpdateState"/> on the existing window</description>
/// </item>
/// <item>
/// <description>The existing window updates its internal state and refreshes its display</description>
/// </item>
/// <item>
/// <description>The existing window is brought to the foreground and activated</description>
/// </item>
/// </list>
/// <para><b>Example Scenario:</b></para>
/// A manager opens the OrderListWindow filtered for "Late Orders". Later, they click a button
/// to view "All Orders". Instead of opening a second window, the existing window's filters
/// are cleared and the list is refreshed to show all orders.
/// <para><b>Implementation Guidelines:</b></para>
/// <list type="bullet">
/// <item>
/// <description>Validate the args array to ensure it contains the expected number and types of parameters</description>
/// </item>
/// <item>
/// <description>Update all relevant properties that affect the window's display state</description>
/// </item>
/// <item>
/// <description>Trigger any necessary data refresh operations</description>
/// </item>
/// <item>
/// <description>Handle null or invalid parameters gracefully</description>
/// </item>
/// <item>
/// <description>Ensure thread-safe operations if updating from non-UI threads</description>
/// </item>
/// </list>
/// <para><b>Benefits:</b></para>
/// <list type="bullet">
/// <item><description>Prevents multiple instances of the same window type</description></item>
/// <item><description>Maintains user's window position and size preferences</description></item>
/// <item><description>Provides smoother user experience without window flicker</description></item>
/// <item><description>Reduces memory usage by reusing existing window instances</description></item>
/// <item><description>Preserves window state like scroll position and expanded sections</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public partial class OrderListWindow : Window, IWindowUpdater
/// {
///     public void UpdateState(params object?[] args)
///     {
///         if (args != null &amp;&amp; args.Length >= 3)
///         {
///             // Update filter properties
///             SelectedOrderStatusFilter = args[0] as BO.OrderStatus?;
///             SelectedScheduleFilter = args[1] as BO.ScheduleStatus?;
///             SelectedTypeFilter = args[2] as BO.TypeOfOrder?;
///             
///             // Refresh the list with new filters
///             LoadOrders();
///         }
///     }
/// }
/// </code>
/// <para>Usage with OpenOrActivateWindow:</para>
/// <code>
/// // This will either open a new window or update an existing one
/// Tools.OpenOrActivateWindow&lt;OrderListWindow&gt;(
///     matchPredicate: null,
///     owner: this,
///     args: new object?[] { orderStatus, scheduleStatus, typeOfOrder }
/// );
/// </code>
/// </example>
/// <seealso cref="Tools.OpenOrActivateWindow{T}(Predicate{T}?, Window?, object?[])"/>
public interface IWindowUpdater
{
    /// <summary>
    /// Updates the window's state with the provided arguments.
    /// </summary>
    /// <param name="args">
    /// A variable-length array of arguments that define the new state of the window.
    /// The specific arguments and their order are defined by each implementing class.
    /// Can contain null values if certain parameters should be reset or cleared.
    /// </param>
    /// <remarks>
    /// <para><b>Implementation Responsibilities:</b></para>
    /// Implementing classes should:
    /// <list type="bullet">
    /// <item>
    /// <description>Document the expected arguments in their class-level documentation</description>
    /// </item>
    /// <item>
    /// <description>Validate the args array before accessing its elements</description>
    /// </item>
    /// <item>
    /// <description>Handle cases where args is null or has insufficient elements</description>
    /// </item>
    /// <item>
    /// <description>Cast arguments to their expected types safely</description>
    /// </item>
    /// <item>
    /// <description>Update all relevant window state based on the arguments</description>
    /// </item>
    /// <item>
    /// <description>Refresh the window's display to reflect the new state</description>
    /// </item>
    /// <item>
    /// <description>Ensure thread-safe execution if called from non-UI threads</description>
    /// </item>
    /// </list>
    /// <para><b>Parameter Validation Example:</b></para>
    /// <code>
    /// public void UpdateState(params object?[] args)
    /// {
    ///     // Validate argument count
    ///     if (args == null || args.Length &lt; 2)
    ///         return;
    ///     
    ///     // Safely cast arguments
    ///     var courierId = args[0] as int?;
    ///     var orderStatus = args[1] as BO.OrderStatus?;
    ///     
    ///     // Only update if valid values provided
    ///     if (courierId.HasValue)
    ///         CurrentCourierId = courierId.Value;
    ///     
    ///     SelectedStatusFilter = orderStatus;
    ///     
    ///     // Refresh display
    ///     RefreshData();
    /// }
    /// </code>
    /// <para><b>Thread Safety:</b></para>
    /// If the UpdateState method is called from a non-UI thread, implementations should
    /// marshal UI updates to the UI thread using Dispatcher.Invoke or Dispatcher.BeginInvoke.
    /// <para><b>Error Handling:</b></para>
    /// Implementations should not throw exceptions for invalid arguments. Instead, they should
    /// validate inputs and gracefully handle unexpected values by either using defaults or
    /// leaving the current state unchanged.
    /// </remarks>
    /// <example>
    /// <para>Simple implementation with two arguments:</para>
    /// <code>
    /// public void UpdateState(params object?[] args)
    /// {
    ///     if (args?.Length >= 2)
    ///     {
    ///         CourierId = (int?)args[0];
    ///         FilterStatus = args[1] as BO.OrderStatus?;
    ///         RefreshList();
    ///     }
    /// }
    /// </code>
    /// <para>Thread-safe implementation:</para>
    /// <code>
    /// public void UpdateState(params object?[] args)
    /// {
    ///     Dispatcher.BeginInvoke(() =>
    ///     {
    ///         if (args?.Length >= 1)
    ///         {
    ///             SelectedFilter = args[0] as BO.FilterType?;
    ///             ReloadData();
    ///         }
    ///     });
    /// }
    /// </code>
    /// </example>
    void UpdateState(params object?[] args);
}
