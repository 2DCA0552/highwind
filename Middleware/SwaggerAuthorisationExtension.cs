/*
 * File: Middleware\SwaggerAuthorisationExtension.cs
 * Created Date: 2019-04-17 10:10:32 +10:00
 * Author: Simon B
 * Last Modified: 2019-04-17 10:12:22 +10:00
 * Modified By: Simon B
 * Copyright (c) 2019 AGD
 * HISTORY:
 */


using Microsoft.AspNetCore.Builder;

namespace Highwind.Middleware
{
    public static class SwaggerAuthorisationExtension
    {
        public static IApplicationBuilder UseSwaggerAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerAuthorisationMiddleware>();
        }
    }
}