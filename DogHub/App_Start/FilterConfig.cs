using System.Web;
using System.Web.Mvc;

namespace DogHub
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            //Enforce authentication site-wide
            filters.Add(new AuthorizeAttribute());
        }
    }
}
