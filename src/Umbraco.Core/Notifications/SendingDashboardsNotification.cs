using System.Collections.Generic;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Notifications
{
    public class SendingDashboardsNotification : INotification
    {
        public IUmbracoContext UmbracoContext { get; }

        public IEnumerable<Tab<IDashboardSlim>> Dashboards { get; }

        public SendingDashboardsNotification(IEnumerable<Tab<IDashboardSlim>> dashboards, IUmbracoContext umbracoContext)
        {
            Dashboards = dashboards;
            UmbracoContext = umbracoContext;
        }
    }
}
