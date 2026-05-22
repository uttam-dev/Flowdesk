using FlowDesk.Application;
using FlowDesk.Application.Common.Validators;
using FlowDesk.Domain;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
using FlowDesk.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using Microsoft.OpenApi.Models;

namespace FlowDesk.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
        {

            services
            .AddApplication()
            .AddDomain()
            .AddInfrastructure(configuration);

            // add scalar
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            //Fluent Validation
            services.AddValidatorsFromAssemblyContaining<UserValidator>();
            services.AddFluentValidationAutoValidation();

            //Jwt Authentication config
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Custom response for unauthorized and forbidden requests
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                        {
                            StatusCode = 401,
                            Message = "Unauthorized",
                            ErrorCode = "UNAUTHORIZED",
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new ErrorResponseDto
                        {
                            StatusCode = 403,
                            Message = "Forbidden",
                            ErrorCode = "FORBIDDEN",
                            TraceId = context.HttpContext.TraceIdentifier
                        });
                    },
                    OnMessageReceived = context =>
                    {
                        string token = null;

                        // 1️ Check Authorization header first (mobile apps, Postman, external APIs)
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            token = authHeader.Substring(7);

                        // 2️ Fallback to HttpOnly cookie (web browser)
                        if (string.IsNullOrEmpty(token))
                            token = context.Request?.Cookies[configuration["CookieOptions:AccessTokenName"]!]!;

                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };

            });

            // Add policy for role-based authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(RoleEnum.Admin.ToString()));
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole(RoleEnum.Manager.ToString()));
                options.AddPolicy("RequireSupportRole", policy => policy.RequireRole(RoleEnum.Support.ToString()));
                options.AddPolicy("RequireEmployeeRole", policy => policy.RequireRole(RoleEnum.Employee.ToString()));
                options.AddPolicy("RequireApprovel", policy => policy.RequireRole(RoleEnum.Manager.ToString()));
                options.AddPolicy("CanCreateRequest", policy => policy.RequireRole(RoleEnum.Employee.ToString(), RoleEnum.Manager.ToString()));
                options.AddPolicy("CanCreateRequestComment", policy => policy.RequireRole(RoleEnum.Admin.ToString(), RoleEnum.Support.ToString(), RoleEnum.Manager.ToString()));
            });

            // Add cors policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                    .WithOrigins("http://localhost:5173", "https://localhost:5173",
                    "http://localhost:5174", "https://localhost:5174")
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
