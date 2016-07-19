using System.IO;
using System.Security.Cryptography.X509Certificates;
using Host.Configuration;
using Host.Cors;
using Host.EntityFrameworkCore;
using Host.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using RigoFunc.ApiCore.Filters;
using RigoFunc.IdentityServer;

namespace Host {
    public class Startup {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env) {
            _environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(Configuration["Data:Default:ConnectionString"]));

            services.AddIdentity<AppUser, IdentityRole<int>>(options => {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext, int>()
            .AddDefaultTokenProviders();

            // Sms and email services
            services.AddSmsEmailService(options => {
                options.SmsApiUrl = Configuration["Services:SendSmsApiUlr"];
                options.EmailApiUrl = Configuration["Services:SendEmailApiUlr"];
            });

            // Use RigoFunc.Account default account service.
            services.UseDefaultAccountService<AppUser>(options => {
                options.DefaultClientId = "system";
                options.DefaultClientSecret = "secret";
                options.DefaultScope = "doctor consultant finance order payment";
                options.CodeSmsTemplate = "SmsTemplate";
                options.PasswordSmsTemplate = "PassTemplate";
            });

            services.AddDistributedSqlServerCache(options => {
                options.ConnectionString = Configuration["Data:Default:ConnectionString"];
                options.SchemaName = "dbo";
                options.TableName = "TokenCache";
            });

            var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");
            var builder = services.AddIdentityServer()
                .SetSigningCredential(cert)
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryScopes(Scopes.Get())
                .AddCustomGrantValidator<CustomGrantValidator>()
                .AddDistributedStores()
                .ConfigureAspNetCoreIdentity<AppUser>()
                // what a fuck solution
                .FixCorsIssues(options => {
                    options.AllowAnyOrigin = true;
                }, new string[] {
                    "api/account/lockout",
                    "api/account/register",
                    "api/account/sendcode",
                    "api/account/login",
                    "api/account/verifycode",
                    "api/account/changepassword",
                    "api/account/resetpassword",
                    "api/account/update",
                    "api/weixin/bind",
                    "api/weixin/login"
                });

            // for the UI
            services
                .AddMvc()
                .AddJsonOptions(options => {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                }).AddMvcOptions(options => {
                    //var policy = new AuthorizationPolicyBuilder()
                    //     .RequireAuthenticatedUser()
                    //     .Build();
                    //options.Filters.Add(new AuthorizeFilter(policy));
                    options.Filters.Add(new ApiResultFilterAttribute());
                    options.Filters.Add(new ApiExceptionFilterAttribute());
                })
                .AddRazorOptions(razor => {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander());
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) {
            //loggerFactory.AddConsole((scope, level) => scope.StartsWith("IdentityServer"));
            //loggerFactory.AddDebug((scope, level) => scope.StartsWith("IdentityServer"));
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseIdentity();
            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}