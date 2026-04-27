using Microsoft.EntityFrameworkCore;
using NewsletterService.Data;
using NewsletterService.Models;
using NewsletterService.Services;

var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<NewsletterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Email Settings ─────────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// ── Email Service ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// ── Controllers & Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseMiddleware<NewsletterService.Middlewares.GlobalExceptionMiddleware>();
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

app.MapGet("/", () => new
{
    Service   = "InkWell NewsletterService",
    Version   = "1.0.0",
    Status    = "Running",
    Timestamp = DateTime.UtcNow
});

app.Run();




