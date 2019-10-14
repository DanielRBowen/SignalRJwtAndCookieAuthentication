using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRJwtAndCookieAuthentication.Data
{
    public class SeedData
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-2.2
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="testUserPw"></param>
        /// <returns></returns>
		public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(
               serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                //context.Database.EnsureCreated();

                // For sample purposes seed both with the same password.
                // Password is set with the following:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything

                if (context.Users.Any())
                {
                    return;
                }
                else
                {
                    var testId = await EnsureUser(serviceProvider, testUserPw, "test@test.com", "test@test.com");
                    await EnsureRole(serviceProvider, testId, "admin");
                }
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                 string testUserPw, string userName, string email = null)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    user = new IdentityUser { UserName = userName, Email = email };
                }
                else
                {
                    user = new IdentityUser { UserName = userName };
                }

                user.EmailConfirmed = true;
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                                      string uid, string role)
        {
            try
            {
                IdentityResult IR = null;
                var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

                if (roleManager == null)
                {
                    throw new Exception("roleManager null");
                }

                if (!await roleManager.RoleExistsAsync(role))
                {
                    IR = await roleManager.CreateAsync(new IdentityRole(role));
                }

                var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                var user = await userManager.FindByIdAsync(uid);

                IR = await userManager.AddToRoleAsync(user, role);

                return IR;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }

        }
    }
}
