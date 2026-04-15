using Microsoft.EntityFrameworkCore;
using ProposalService.Data;
using ProposalService.Services;
using WorkService.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ServiceProposal>();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = null;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddScoped<NotificationServiceClient>();
builder.Services.AddScoped<WorkServiceClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpClient<NotificationServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var enabled = config.GetValue<bool>("ExternalServices:NotificationServiceEnabled");

    if (!enabled) return;

    var url = config["ExternalServices:NotificationService"];
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("NotificationService URL is not configured.");

    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient<WorkServiceClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var url = config["ExternalServices:WorkService"];
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("WorkService URL is not configured.");

    client.BaseAddress = new Uri(url);
});


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseExceptionHandler();
app.UseStatusCodePages();
//app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
