**Example startup.cs in dotnet core app**
```c#
public class Startup
{
    private SecurityKey _issuerSigningKey;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        if(Boolean.Parse(Configuration["Token:useRsa"])){
            InitializeRsa();
        }
        else{
            InitializeHmac();
        }
    }

    private void InitializeRsa()
    {
        var publicKeyXml = File.ReadAllText(Configuration["Token:rsaPublicKeyXml"]);
        var publicRsa = XmlHelper.FromXmlString(publicKeyXml);

        _issuerSigningKey = new RsaSecurityKey(publicRsa);
    }

    private void InitializeHmac()
    {
        _issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:hmacSecretKey"]));
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = Configuration["Token:issuer"],
            ValidAudience = Configuration["Token:audience"],
            ValidateIssuer = Boolean.Parse(Configuration["Token:validateIssuer"]),
            ValidateIssuerSigningKey = Boolean.Parse(Configuration["Token:validateIssuerSigningKey"]),
            ValidateAudience = Boolean.Parse(Configuration["Token:validateAudience"]),
            ValidateLifetime = Boolean.Parse(Configuration["Token:validateLifetime"]),
            IssuerSigningKey = _issuerSigningKey
        };
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[Configuration["Token:tokenName"]];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = tokenValidationParameters;
            });
        
        // Settings
        services.Configure<TokenSettings>(Configuration.GetSection("Token"));

        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();

        // Custom Highwind Auth
        app.UseHighwindAuth();

        app.UseAuthentication();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
```

**Example UseHighwindAuth middleware in dotnet core client app**
```c#
public class HighwindAuthMiddleware
{
    private readonly TokenSettings _settings;
    private readonly RequestDelegate _next;

    public HighwindAuthMiddleware(RequestDelegate next, IOptions<TokenSettings> settings)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if(!context.Request.Cookies.ContainsKey(_settings.TokenName)){
            var absoluteUri = string.Concat(
                    context.Request.Scheme,
                    "://",
                    context.Request.Host.ToUriComponent(),
                    context.Request.PathBase.ToUriComponent(),
                    context.Request.Path.ToUriComponent(),
                    context.Request.QueryString.ToUriComponent());

            context.Response.Redirect($"{_settings.Issuer}{_settings.IssuerAuthPath}?apiKey={_settings.ApiKey}&redirectURL={HttpUtility.UrlEncode(absoluteUri)}");
            return;
        }

        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}
public static class HighwindAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseHighwindAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HighwindAuthMiddleware>();
    }
}
```