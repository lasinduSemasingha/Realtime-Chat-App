
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Common;
using System.ComponentModel;

namespace API.Endpoints
{
    public static class AccountEndpoint
    {
        public static RouteGroupBuilder MapAccountEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/account").WithTags("Account");

            group.MapPost("/register", async (HttpContext context, UserManager<AppUser> userManager, [FromForm] string fullName,
                [FromForm] string email, [FromForm] string password) =>
            {
                var userFromDb = await userManager.FindByEmailAsync(email);
                if (userFromDb is not null)
                {
                    return Results.BadRequest(Response<string>.Failure("User already exists"));
                }

                var user = new AppUser
                {
                    Email = email,
                    FullName = fullName
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return Results.BadRequest(Response<string>.Failure(result.
                        Errors.Select(x => x.Description).FirstOrDefault()!));
                }

                return Results.Ok(Response<string>.Success("","User created successfully"));
            });
            return group;
        }
    }
}
