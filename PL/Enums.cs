using System.Collections;
using System.Collections.Generic;

namespace PL;

internal class CourierFieldSort : IEnumerable
{
    static readonly IEnumerable<BO.CourierFieldSort> s_enums =
        (Enum.GetValues(typeof(BO.CourierFieldSort)) as IEnumerable<BO.CourierFieldSort>)!;

    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}

internal class CourierFieldFilter : IEnumerable
{
    static readonly IEnumerable<BO.CourierFieldFilter> s_enums =
        (Enum.GetValues(typeof(BO.CourierFieldFilter)) as IEnumerable<BO.CourierFieldFilter>)!;

    IEnumerator IEnumerable.GetEnumerator() => s_enums.GetEnumerator();
}
