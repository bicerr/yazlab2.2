using Yarp.ReverseProxy;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoClient = new MongoClient("mongodb://mongodb:27017");
var database = mongoClient.GetDatabase("dispatcher_db");
var logsCollection = database.GetCollection<LogEntry>("logs");

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton(logsCollection);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Loglama middleware
app.Use(async (context, next) =>
{
    var log = new LogEntry
    {
        Method = context.Request.Method,
        Path = context.Request.Path,
        Timestamp = DateTime.UtcNow,
        StatusCode = context.Response.StatusCode
    };

    await next();

    log.StatusCode = context.Response.StatusCode;
    await logsCollection.InsertOneAsync(log);
});

// JWT Auth kontrolü
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";

    if (!path.StartsWith("/auth"))
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
    }

    await next();
});

app.MapReverseProxy();

app.Run();

public class LogEntry
{
    public MongoDB.Bson.ObjectId Id { get; set; }
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public int StatusCode { get; set; }
}