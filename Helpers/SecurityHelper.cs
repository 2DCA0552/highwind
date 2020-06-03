/*
 * File: Helpers\SecurityHelper.cs
 * Created Date: 2019-04-17 09:56:39 +10:00
 * Author: Simon B
 * Last Modified: 2019-04-17 10:04:01 +10:00
 * Modified By: Simon B
 * Copyright (c) 2019 AGD
 * HISTORY:
 */


using System;
using System.Security.Principal;
using Highwind.Helpers.Interfaces;
using Highwind.Settings;
using Microsoft.Extensions.Options;

namespace Highwind.Helpers
{
    public class SecurityHelper : ISecurityHelper
    {
        private readonly SecuritySettings _settings;

        public SecurityHelper(IOptions<SecuritySettings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool IsAdmin(WindowsIdentity identity)
        {
            return IsAdmin(new WindowsPrincipal(identity));
        }

        private bool IsAdmin(IPrincipal principal)
        {
            return principal.IsInRole(_settings.AdminGroup);
        }
    }
}