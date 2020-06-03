using System.Collections.Generic;
using System.Security.Principal;

namespace Highwind.Helpers.Interfaces
{
    public interface IJwtHelper
    {
        string Create(IIdentity identity, string audience, List<string> appGroupPrefixes);
        bool ValidateToken(string jwt, string validAudience);
        dynamic ReadToken(string jwt);
    }
}