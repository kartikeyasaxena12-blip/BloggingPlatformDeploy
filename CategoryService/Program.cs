using Microsoft.EntityFrameworkCore;
using CategoryService.Data;
using CategoryService.Models;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<CategoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Controllers & Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseMiddleware<CategoryService.Middlewares.GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Seed Categories ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CategoryDbContext>();
    context.Database.EnsureCreated();
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Technology" },
            new Category { Name = "Lifestyle" },
            new Category { Name = "Food" },
            new Category { Name = "Travel" },
            new Category { Name = "Business" }
        );
        context.SaveChanges();
    }
}

app.Run();



