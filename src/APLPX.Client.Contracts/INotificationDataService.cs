using APLPX.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APLPX.Client.Contracts
{
    public interface INotificationDataService
    {
        void SendEmail(List<string> toAddresses, string subject, string body, bool isHtml);
        void SendEmail(UserRoleType role, string subject, string body, bool isHtml);
    }
}
