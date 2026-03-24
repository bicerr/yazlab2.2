using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoClient = new MongoClient("mongodb://mongodb:27017");
var database = mongoClient.GetDatabase("product_db");
var productsCollection = database.GetCollection<Product>("products");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tüm ürünleri getir
app.MapGet("/products", async () =>
{
    var products = await productsCollection.Find(_ => true).ToListAsync();
    return Results.Ok(products);
});

// Tek ürün getir
app.MapGet("/products/{id}", async (string id) =>
{
    var product = await productsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    if (product == null) return Results.NotFound("Ürün bulunamadı.");
    return Results.Ok(product);
});

// Ürün ekle
app.MapPost("/products", async (ProductRequest req) =>
{
    var product = new Product
    {
        Name = req.Name,
        Price = req.Price,
        Stock = req.Stock,
        Category = req.Category
    };
    await productsCollection.InsertOneAsync(product);
    return Results.Created($"/products/{product.Id}", product);
});

// Ürün güncelle
app.MapPut("/products/{id}", async (string id, ProductRequest req) =>
{
    var update = Builders<Product>.Update
        .Set(p => p.Name, req.Name)
        .Set(p => p.Price, req.Price)
        .Set(p => p.Stock, req.Stock)
        .Set(p => p.Category, req.Category);

    var result = await productsCollection.UpdateOneAsync(p => p.Id == id, update);
    if (result.MatchedCount == 0) return Results.NotFound("Ürün bulunamadı.");
    return Results.Ok("Ürün güncellendi.");
});

// Ürün sil
app.MapDelete("/products/{id}", async (string id) =>
{
    var result = await productsCollection.DeleteOneAsync(p => p.Id == id);
    if (result.DeletedCount == 0) return Results.NotFound("Ürün bulunamadı.");
    return Results.Ok("Ürün silindi.");
});

app.Run();

public class Product
{
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = "";
}

public record ProductRequest(string Name, decimal Price, int Stock, string Category);