/*
 * File: Middleware\SwaggerAuthorisationMiddleware.cs
 * Created Date: 2019-04-17 10:05:15 +10:00
 * Author: Simon B
 * Last Modified: 2019-04-17 10:23:13 +10:00
 * Modified By: Simon B
 * Copyright (c) 2019 AGD
 * HISTORY:
 */


using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Highwind.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Highwind.Middleware
{
    public class SwaggerAuthorisationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecurityHelper _securityHelper;

        public SwaggerAuthorisationMiddleware(RequestDelegate next, ISecurityHelper securityHelper)
        {
            _next = next;
            _securityHelper = securityHelper ?? throw new ArgumentNullException(nameof(securityHelper));

        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(context.Request.Path.StartsWithSegments("/index.html"))
            {
                var identity = context.User.Identity as WindowsIdentity;
                if(!_securityHelper.IsAdmin(identity))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
            
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}