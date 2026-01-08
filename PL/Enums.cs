using System.Collections;
using System.Collections.Generic;

namespace PL;

/// <summary>
/// Provides an enumerable wrapper for the CourierFieldSort enum values.
/// </summary>
/// <remarks>
/// This class enables XAML binding to the CourierFieldSort enum values by implementing IEnumerable.
/// It is used in ComboBox ItemsSource bindings to display sorting options in the UI.
/// The class provides access to all available sorting fields for courier lists.
/// </remarks>
internal class CourierFieldSort : IEnumerable
{
    /// <summary>
    /// Static collection of all CourierFieldSort enum values.
    /// </summary>
    static readonly IEnumerable<BO.CourierFieldSort> s_enums =
        (Enum.GetValues(typeof(BO.CourierFieldSort)) as IEnumerable<BO.CourierFieldSort>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the CourierFieldSort enum values.
    /// </summary>
    /// <returns>An IEnumerator for the CourierFieldSort enum values.</returns>
    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}

/// <summary>
/// Provides an enumerable wrapper for the CourierFieldFilter enum values.
/// </summary>
/// <remarks>
/// This class enables XAML binding to the CourierFieldFilter enum values by implementing IEnumerable.
/// It is used in ComboBox ItemsSource bindings to display filtering options in the UI.
/// The class provides access to all available filter options for courier lists:
/// <list type="bullet">
/// <item><description>All - Display all couriers regardless of status</description></item>
/// <item><description>IsActive - Display only active couriers</description></item>
/// <item><description>InActive - Display only inactive couriers</description></item>
/// </list>
/// Each enum value has a Hebrew description attribute for UI display purposes.
/// </remarks>
internal class CourierFieldFilter : IEnumerable
{
    /// <summary>
    /// Static collection of all CourierFieldFilter enum values.
    /// </summary>
    static readonly IEnumerable<BO.CourierFieldFilter> s_enums =
        (Enum.GetValues(typeof(BO.CourierFieldFilter)) as IEnumerable<BO.CourierFieldFilter>)!;

    /// <summary>
    /// Returns an enumerator that iterates through the CourierFieldFilter enum values.
    /// </summary>
    /// <returns>An IEnumerator for the CourierFieldFilter enum values.</returns>
    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}
internal class OrderFilterScheduleStatus : IEnumerable
{
    static readonly IEnumerable<BO.ScheduleStatus?> s_enums =
        new BO.ScheduleStatus?[] { null }
        .Concat(Enum.GetValues<BO.ScheduleStatus>().Cast<BO.ScheduleStatus?>());

    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}

internal class OrderFilterTypeOfOrder : IEnumerable
{
    static readonly IEnumerable<BO.TypeOfOrder> s_enums =
        (Enum.GetValues(typeof(BO.TypeOfOrder)) as IEnumerable<BO.TypeOfOrder>)!;

    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}

internal class SelectionItem
{
    public object? Id { get; set; }
    public required string Name { get; set; }
};



