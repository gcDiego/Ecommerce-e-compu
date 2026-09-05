using System.Web;
using System.Web.Mvc;
using CapaPresentacionAdmin.Filtros;

namespace CapaPresentacionAdmin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new NoCacheAttribute());
        }
    }
}
