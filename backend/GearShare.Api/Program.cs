using GearShare.Api.Data;
using GearShare.Api.Models;
using GearShare.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
// sus, la using-uri
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using GearShare.Api.Validation.Items; // pentru tipul CreateItemRequestValidator
using System.Security.Claims; // for RoleClaimType / NameClaimType


var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Identity (Guid keys)
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();   // ✅ needed for GeneratePasswordResetTokenAsync / ResetPasswordAsync

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = key,
            RoleClaimType = ClaimTypes.Role, // ✅ so [Authorize(Roles="...")] works
            NameClaimType = ClaimTypes.Name  // (optional) aligns User.Identity.Name
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IImageStorage, LocalImageStorage>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", p => p
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddScoped<GearShare.Api.Services.IImageStorage, GearShare.Api.Services.LocalImageStorage>();
// after builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation();      // rulează validarea automat în pipeline
builder.Services.AddFluentValidationClientsideAdapters();  // opțional (pentru adaptor client-side)

builder.Services.AddValidatorsFromAssemblyContaining<CreateItemRequestValidator>();
// sau: builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GearShare API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseStaticFiles(); // wwwroot

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Don't force HTTPS in dev
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations + seed at startup (dev only)
using (var scope = app.Services.CreateScope())
{
    var sp      = scope.ServiceProvider;
    var db      = sp.GetRequiredService<AppDbContext>();
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

    await db.Database.MigrateAsync();

    // Roles
    async Task EnsureRole(string r)
    {
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole<Guid>(r));
    }
    await EnsureRole("ADMIN");
    await EnsureRole("OWNER");
    await EnsureRole("RENTER");

    // Users (force known password in DEV)
    async Task<ApplicationUser> EnsureUser(string email, string password, string display, params string[] roles)
    {
        var u = await userMgr.FindByEmailAsync(email);
        if (u == null)
        {
            u = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                DisplayName = display,
                EmailConfirmed = true
            };
            var create = await userMgr.CreateAsync(u);
            if (!create.Succeeded)
                throw new Exception("Create user failed: " + string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        // Ensure not locked
        u.EmailConfirmed = true;
        u.LockoutEnabled = false;
        u.LockoutEnd = null;
        u.AccessFailedCount = 0;
        await userMgr.UpdateAsync(u);

        // Reset password (dev-safe)
        var resetToken = await userMgr.GeneratePasswordResetTokenAsync(u);
        var reset = await userMgr.ResetPasswordAsync(u, resetToken, password);
        if (!reset.Succeeded)
            throw new Exception("Reset password failed: " + string.Join("; ", reset.Errors.Select(e => e.Description)));

        // Ensure roles
        foreach (var r in roles)
            if (!await userMgr.IsInRoleAsync(u, r))
                await userMgr.AddToRoleAsync(u, r);

        return u;
    }

    // Known dev credentials
    await EnsureUser("admin@gearshare.local",  "Admin123!",  "Admin",  "ADMIN");
    await EnsureUser("owner@gearshare.local",  "Owner123!",  "Owner",  "OWNER");
    await EnsureUser("renter@gearshare.local", "Renter123!", "Renter", "RENTER");
}

// keep your data seeding if you want demo items/listings
await SeedData.EnsureAsync(app.Services);

app.Run();
