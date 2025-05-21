
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Common;
using API.Services;
using API.DTOs;

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

            group.MapPost("/login", async (UserManager<AppUser> UserManager,
                TokenService tokenService, LoginDto dto) =>
            {
                if (dto is null)
                {
                    return Results.BadRequest(Response<string>.Failure("Invalid login attempt"));
                }

                var user = await UserManager.FindByEmailAsync(dto.Email);
                if (user is null)
                {
                    return Results.BadRequest(Response<string>.Failure("User not found."));
                }

                var result = await UserManager.CheckPasswordAsync(user!, dto.Password);

                if (!result)
                {
                    return Results.BadRequest(Response<string>.Failure("Invalid password."));
                }

                var token = tokenService.GenerateToken(user.Id, user.UserName!);

                return Results.Ok(Response<string>.Success(token, "Login successfully!"));
            });
            return group;
        }
    }
}
