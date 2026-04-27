using Microsoft.EntityFrameworkCore;
using PostService.Data;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<PostDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Controllers & Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Http Client ────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseMiddleware<PostService.Middlewares.GlobalExceptionMiddleware>();
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

// ── Startup Info ───────────────────────────────────────────────────────────
app.MapGet("/", () => new
{
    Service = "InkWell PostService",
    Version = "1.0.0",
    Status = "Running",
    Timestamp = DateTime.UtcNow
});

app.Run();

