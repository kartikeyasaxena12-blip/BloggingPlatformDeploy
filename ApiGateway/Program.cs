var builder = WebApplication.CreateBuilder(args);

// ── CORS ──────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── YARP ───────────────────────────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── Health Checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────────────
app.UseMiddleware<ApiGateway.Middlewares.GlobalExceptionMiddleware>();
app.UseCors("AllowAll");
app.MapHealthChecks("/health");

// ── Gateway Info Endpoint ──────────────────────────────────────────────────
app.MapGet("/", () => new
{
    Service = "InkWell ApiGateway",
    Version = "1.0.0",
    Status = "Running",
    Routes = new[]
    {
        "POST /api/auth/** → AuthService:5101",
        "GET|POST /api/posts/** → PostService:5201",
        "GET|POST /api/comments/** → CommentService:5301",
        "GET|POST /api/categories/** → CategoryService:5401",
        "GET|POST /api/newsletter/** → NewsletterService:5601",
        "GET|POST /api/notifications/** → NotificationService:5701"
    },
    Timestamp = DateTime.UtcNow
});

app.MapReverseProxy();

app.Run();
