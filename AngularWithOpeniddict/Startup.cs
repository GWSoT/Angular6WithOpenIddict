using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AngularWithOpeniddict.Data;
using AngularWithOpeniddict.WriteModels;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace AngularWithOpeniddict
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;
        private readonly X509Certificate2 _certificate;

        public Startup(IHostingEnvironment env)
        {
            _certificate = new X509Certificate2(Path.Combine(env.ContentRootPath, "cert.pfx"), "");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            _environment = env;
            Configuration = builder.Build();

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
                
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Email;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
            });


            services.AddOpenIddict()
                    .AddCore(options =>
                    {
                        options.UseEntityFrameworkCore()
                                .UseDbContext<ApplicationDbContext>();
                    })
                    .AddServer(options =>
                    {
                        options.UseMvc();
                        options.RegisterScopes(OpenIdConnectConstants.Scopes.Email,
                       OpenIdConnectConstants.Scopes.Profile,
                       OpenIddictConstants.Scopes.Roles);
                        options.EnableAuthorizationEndpoint("/connect/authorize")
                               .EnableLogoutEndpoint("/connect/logout")
                               .EnableUserinfoEndpoint("/connect/userinfo")
                               .EnableIntrospectionEndpoint("/connect/introspect");

                        options.AllowImplicitFlow();

                        options.AddEphemeralSigningKey();

                        options.DisableHttpsRequirement();

                        options.AddSigningCertificate(_certificate);

                        options.UseJsonWebTokens();
                    });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = "http://localhost:12345";
                    options.Audience = "resources";
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = OpenIdConnectConstants.Claims.Name,
                        RoleClaimType = OpenIdConnectConstants.Claims.Role
                    };
                });

            services.AddCors(options => 
            {
                options.AddPolicy("AllowAnyPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseCors("AllowAnyPolicy");
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

            InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync(IServiceProvider services)
        {
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();

                await CreateApplicationsAsync();
                await CreateScopesAsync();

                async Task CreateApplicationsAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<OpenIddictApplicationManager<OpenIddictApplication>>();

                    if (await manager.FindByClientIdAsync("angular6") == null)
                    {
                        var application = new OpenIddictApplicationDescriptor
                        {
                            ClientId = "angular6",
                            DisplayName = "Angular SPA",
                            PostLogoutRedirectUris = { new Uri("http://localhost:9000/account/sign-out") },
                            RedirectUris = { new Uri("http://localhost:9000/account/sign-in") },
                            Permissions =
                            {
                                OpenIddictConstants.Permissions.Endpoints.Authorization,
                                OpenIddictConstants.Permissions.Endpoints.Logout,
                                OpenIddictConstants.Permissions.GrantTypes.Implicit,
                                OpenIddictConstants.Permissions.Scopes.Email,
                                OpenIddictConstants.Permissions.Scopes.Profile,
                                OpenIddictConstants.Permissions.Scopes.Roles,
                                OpenIddictConstants.Permissions.Prefixes.Scope + "api1"
                            }

                        };

                        await manager.CreateAsync(application);
                    }

                    if (await manager.FindByClientIdAsync("resources") == null)
                    {
                        var application = new OpenIddictApplicationDescriptor
                        {
                            ClientId = "resources",
                            ClientSecret = "77be52c7-06a2-4830-90bc-715b03b97119",
                            Permissions =
                            {
                                OpenIddictConstants.Permissions.Endpoints.Introspection
                            }
                        };

                        await manager.CreateAsync(application);
                    }
                }

                async Task CreateScopesAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<OpenIddictScopeManager<OpenIddictScope>>();

                    if (await manager.FindByNameAsync("api1") == null)
                    {
                        var descriptor = new OpenIddictScopeDescriptor
                        {
                            Name = "api1",
                            Resources = { "resources" }
                        };

                        await manager.CreateAsync(descriptor);
                    }
                }
            }
        }
    }
}
