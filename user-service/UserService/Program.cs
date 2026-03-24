using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoClient = new MongoClient("mongodb://mongodb:27017");
var database = mongoClient.GetDatabase("order_db");
var ordersCollection = database.GetCollection<Order>("orders");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tüm siparişleri getir
app.MapGet("/orders", async () =>
{
    var orders = await ordersCollection.Find(_ => true).ToListAsync();
    return Results.Ok(orders);
});

// Tek sipariş getir
app.MapGet("/orders/{id}", async (string id) =>
{
    var order = await ordersCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
    if (order == null) return Results.NotFound("Sipariş bulunamadı.");
    return Results.Ok(order);
});

// Sipariş oluştur
app.MapPost("/orders", async (OrderRequest req) =>
{
    var order = new Order
    {
        ProductId = req.ProductId,
        Quantity = req.Quantity,
        CustomerEmail = req.CustomerEmail,
        Status = "Beklemede",
        CreatedAt = DateTime.UtcNow
    };
    await ordersCollection.InsertOneAsync(order);
    return Results.Created($"/orders/{order.Id}", order);
});

// Sipariş güncelle
app.MapPut("/orders/{id}", async (string id, OrderStatusRequest req) =>
{
    var update = Builders<Order>.Update.Set(o => o.Status, req.Status);
    var result = await ordersCollection.UpdateOneAsync(o => o.Id == id, update);
    if (result.MatchedCount == 0) return Results.NotFound("Sipariş bulunamadı.");
    return Results.Ok("Sipariş güncellendi.");
});

// Sipariş sil
app.MapDelete("/orders/{id}", async (string id) =>
{
    var result = await ordersCollection.DeleteOneAsync(o => o.Id == id);
    if (result.DeletedCount == 0) return Results.NotFound("Sipariş bulunamadı.");
    return Results.Ok("Sipariş silindi.");
});

app.Run();

public class Order
{
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public record OrderRequest(string ProductId, int Quantity, string CustomerEmail);
public record OrderStatusRequest(string Status);