using CommerceBankWebApp.Areas.Identity.Data;
using CommerceBankWebApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetService<UserManager<CommerceBankWebAppUser>>();
                var roleManager = services.GetService<RoleManager<IdentityRole>>();

                context.Database.EnsureCreated();

                EnsureAdminCreated(userManager, roleManager).Wait();

                context.SaveChanges();
            }

            host.Run();
        }

        private static async Task EnsureAdminCreated(UserManager<CommerceBankWebAppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
                var user = await userManager.FindByNameAsync("fcbiadmin@fcbi.com");

                if (user == null)
                {
                    user = new CommerceBankWebAppUser
                    {
                        UserName = "fcbiadmin@fcbi.com",
                        EmailConfirmed = true,
                    };

                    await userManager.CreateAsync(user, "Wifokato68!");
            }

            if (!await roleManager.RoleExistsAsync("admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
                await userManager.AddToRoleAsync(user, "admin");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
