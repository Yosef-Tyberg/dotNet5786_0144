using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PL;

public class DeliveryTypesCollection : IEnumerable
{
    static readonly IEnumerable<object> s_enums =
        (new object[] { "None" })
        .Concat(Enum.GetValues(typeof(BO.DeliveryTypes)).Cast<object>());

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

public class OrderTypesCollection : IEnumerable
{
    static readonly IEnumerable<object> s_enums =
        (new object[] { "None" })
        .Concat(Enum.GetValues(typeof(BO.OrderTypes)).Cast<object>());

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

public class DeliveryEndTypesCollection : IEnumerable
{
    static readonly IEnumerable<object> s_enums =
        (new object[] { "None" })
        .Concat(Enum.GetValues(typeof(BO.DeliveryEndTypes)).Cast<object>());

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

public class OrderStatusCollection : IEnumerable
{
    static readonly IEnumerable<object> s_enums =
        (new object[] { "None" })
        .Concat(Enum.GetValues(typeof(BO.OrderStatus)).Cast<object>());

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

public class ScheduleStatusCollection : IEnumerable
{
    static readonly IEnumerable<object> s_enums =
        (new object[] { "None" })
        .Concat(Enum.GetValues(typeof(BO.ScheduleStatus)).Cast<object>());

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}