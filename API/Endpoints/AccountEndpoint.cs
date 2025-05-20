
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Common;
using API.Services;

namespace API.Endpoints
{
    public static class AccountEndpoint
    {
        public static RouteGroupBuilder MapAccountEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/account").WithTags("Account");

            group.MapPost("/register", async (HttpContext context, UserManager<AppUser> userManager, [FromForm] string username, [FromForm] string fullName,
                [FromForm] string email, [FromForm] string password, [FromForm] IFormFile? profileImage) =>
            {
                var userFromDb = await userManager.FindByEmailAsync(email);
                if (userFromDb is not null)
                {
                    return Results.BadRequest(Response<string>.Failure("User already exists"));
                }

                if (profileImage is null)
                {
                    return Results.BadRequest(Response<string>.Failure("Profile image is required"));
                }

                var picture = await FileUpload.Upload(profileImage);

                picture = $"{context.Request.Scheme}://{context.Request.Host}/uploads/{picture}";

                var user = new AppUser
                {
                    UserName = username,
                    Email = email,
                    FullName = fullName,
                    ProfileImage = picture
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
