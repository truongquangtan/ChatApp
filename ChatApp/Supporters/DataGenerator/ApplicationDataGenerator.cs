using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using ChatApp.Data;
using ChatApp.Models;
using ChatApp.Supporters.Constants;

namespace ChatApp.Supporters.DataGenerator
{
    public class ApplicationDataGenerator
    {
        const string DEFAULT = "default";
        public static async Task Initialize(IServiceProvider serviceProvider, string adminName, string adminPassword)
        {
            using (var context = new ChatAppImplementationContext(serviceProvider.GetRequiredService<DbContextOptions<ChatAppImplementationContext>>()))
            {
                await CreateListOfRoleAsync(serviceProvider, new List<String> { RoleName.ADMIN, RoleName.USER, RoleName.COLLABORATOR });

                var adminID = await EnsureUser(serviceProvider, adminPassword, adminName);
                await EnsureRole(serviceProvider, adminID, RoleName.ADMIN);
            };
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, string testUserPw, string userName)
        {
            var userManager = serviceProvider.GetService<UserManager<User>>();

            var user = await userManager.FindByNameAsync(userName);
            if(user == null)
            {
                user = new User()
                {
                    Username = userName,
                    FullName = DEFAULT,
                    Email = DEFAULT,
                    Phone = DEFAULT,
                    StudentId = DEFAULT,
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            if(user == null)
            {
                throw new Exception("The password provided is not strong enough");
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<Role>>();

            if(roleManager == null)
            {
                throw new Exception("Null rolemanager");
            }

            IdentityResult IR;
            if(!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new Role() { Name = role });
            }

            var userManager = serviceProvider.GetService<UserManager<User>>();

            var user = await userManager.FindByIdAsync(uid);

            if(user == null)
            {
                throw new Exception("The password of user is not strong enough");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;

        }

        private static async Task<List<IdentityResult>> CreateListOfRoleAsync(IServiceProvider serviceProvider, List<string> roles)
        {
            var roleManager = serviceProvider.GetService<RoleManager<Role>>();

            if(roleManager == null)
            {
                throw new Exception("Null rolemanager");
            }
            List<IdentityResult> identityResults = new List<IdentityResult>() ;
            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    var identityResult = await roleManager.CreateAsync(new Role() { Name = role });
                    identityResults.Add(identityResult);
                }
            }

            return identityResults;
        }

    }
}
