using MqPlayground.Api.Hubs;
using MqPlayground.Api.Services;
using MqPlayground.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR for real-time messaging
builder.Services.AddSignalR();

// Add CORS for frontend access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register MQ connection service as singleton (shared connection)
builder.Services.AddSingleton<IMqConnectionService, MqConnectionService>();

// Register Queue Browser service for real-time queue visualization
builder.Services.AddSingleton<IQueueBrowserService, QueueBrowserService>();

// Register Point-to-Point services
builder.Services.AddSingleton<PointToPointService>();
builder.Services.AddHostedService<PointToPointConsumer>();

// Register Pub/Sub services
builder.Services.AddSingleton<PubSubService>();
builder.Services.AddHostedService<TopicSubscriber>();

// Register Request/Reply services
builder.Services.AddSingleton<RequestReplyService>();
builder.Services.AddHostedService<ReplyHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapControllers();

// Map SignalR hub
app.MapHub<MessageHub>("/messageHub");

// Connect to MQ on startup
var mqService = app.Services.GetRequiredService<IMqConnectionService>();
await mqService.ConnectAsync();

app.Run();
