/*
 * File: Interfaces\ISecurityHelper.cs
 * Created Date: 2019-04-17 09:59:46 +10:00
 * Author: Simon B
 * Last Modified: 2019-04-17 10:01:11 +10:00
 * Modified By: Simon B
 * Copyright (c) 2019 AGD
 * HISTORY:
 */


using System.Security.Principal;

namespace Highwind.Helpers.Interfaces
{
    public interface ISecurityHelper
    {
        bool IsAdmin(WindowsIdentity identity);
    }
}