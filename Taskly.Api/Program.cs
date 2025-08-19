using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taskly.Api.Data;
using Taskly.Api.Auth;
using Taskly.Api.Models;
using Taskly.Api.Requests;

var builder = WebApplication.CreateBuilder(args);

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=taskly.db"));

// JWT config
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "dev-secret-change-me";
var jwtIssuer = jwtSection["Issuer"] ?? "Taskly";
var jwtAudience = jwtSection["Audience"] ?? "TasklyClient";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddAuthorization();

// CORS: allow Vite dev client
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});


var app = builder.Build();

app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/hello", () => "Hello from Taskly!");

// POST /auth/register
app.MapPost("/auth/register", async (RegisterRequest req, AppDbContext db) =>
{
    var wsId = req.WorkspaceId ?? 1;

    // simple checks
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Username and password required.");

    var exists = await db.Users.AnyAsync(u => u.Username == req.Username);
    if (exists) return Results.BadRequest("Username taken.");

    var (hash, salt) = PasswordHasher.Hash(req.Password);
    var user = new User
    {
        Username = req.Username,
        PasswordHash = hash,
        PasswordSalt = salt,
        WorkspaceId = wsId
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Registered" });
});

// POST /auth/login
app.MapPost("/auth/login", async (LoginRequest req, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
    if (user is null) return Results.Unauthorized();

    var ok = PasswordHasher.Verify(req.Password, user.PasswordSalt, user.PasswordHash);
    if (!ok) return Results.Unauthorized();

    // JWT with workspace_id claim
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("workspace_id", user.WorkspaceId.ToString())
    };

    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60),
        signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new AuthResponse(jwt));
});

// ----- Task endpoints (require JWT) -----

static int GetWorkspaceId(ClaimsPrincipal user)
    => int.Parse(user.FindFirst("workspace_id")?.Value ?? "0");

var tasks = app.MapGroup("/tasks").RequireAuthorization();

// GET /tasks  -> list tasks for this workspace
tasks.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var wsId = GetWorkspaceId(user);
    var items = await db.TaskItems
        .Where(t => t.WorkspaceId == wsId)
        .OrderBy(t => t.Id)
        .ToListAsync();

    return Results.Ok(items);
});

// POST /tasks  -> create task in this workspace
tasks.MapPost("/", async (ClaimsPrincipal user, AppDbContext db, CreateTaskRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest("Title required.");

    var wsId = GetWorkspaceId(user);
    var item = new TaskItem { Title = req.Title, IsComplete = false, WorkspaceId = wsId };
    db.TaskItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
});

// POST /tasks/{id}/toggle  -> flip complete (same workspace only)
tasks.MapPost("/{id:int}/toggle", async (ClaimsPrincipal user, AppDbContext db, int id) =>
{
    var wsId = GetWorkspaceId(user);
    var item = await db.TaskItems.FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == wsId);
    if (item is null) return Results.NotFound();

    item.IsComplete = !item.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// ----- end task endpoints -----



app.Run();
