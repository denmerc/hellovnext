using APLPX.Entity;
using System;
using System.Collections.Generic;

namespace APLPX.Client.Contracts
{
    public interface IAuthenticationDataService
    {
        User Authenticate(string userName, string password);
    }
}
