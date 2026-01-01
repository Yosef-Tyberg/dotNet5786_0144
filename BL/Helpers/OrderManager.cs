
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Helpers;

internal static class OrderManager
{
    private static DalApi.IDal s_dal = DalApi.Factory.Get;
}
