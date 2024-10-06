using System;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Library.Presentation.Authentication;

public static class JwtConfigurar
{
    private static JwtSettings jwtSettings;

    public static void ConfigureJwt(this WebApplicationBuilder builder)
    {
        jwtSettings = builder.Configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

        if (jwtSettings == null || !jwtSettings.Enabled) return;

        builder
            .Services
            .AddAuthentication(ConfigurOptions)
            .AddJwtBearer(ConfigureOptions);
    }

    private static void ConfigureOptions(JwtBearerOptions options)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
        };
    }

    private static void ConfigurOptions(AuthenticationOptions options)
    {
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    }
}
