using Highwind.Helpers;
using Highwind.Helpers.Interfaces;
using Highwind.Middleware;
using Highwind.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace highwind
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Highwind API", Version = "v1" });

                c.IncludeXmlComments("highwind.xml");
            });

            // Authentication & Authorisation
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorsOnly", policy => policy.RequireRole(Configuration["Security:adminGroup"]));
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(Configuration.GetSection("Cors:allowedOrigins").Get<string[]>())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                    .Build());
            });

            // Settings
            services.Configure<SecuritySettings>(Configuration.GetSection("Security"));
            services.Configure<TokenSettings>(Configuration.GetSection("Token"));
            services.Configure<LiteDbSettings>(Configuration.GetSection("LiteDb"));

            // Helpers
            services.AddSingleton<IJwtHelper, JwtHelper>();
            services.AddSingleton<IXmlHelper, XmlHelper>();
            services.AddSingleton<IHashHelper, HashHelper>();
            services.AddSingleton<IDataRepository, LiteDbHelper>();
            services.AddSingleton<ISecurityHelper, SecurityHelper>();

            // API Versioning
            services.AddApiVersioning(o => {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerAuth();

            // CORS
            app.UseCors("CorsPolicy");

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Highwind V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
