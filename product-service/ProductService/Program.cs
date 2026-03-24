using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoClient = new MongoClient("mongodb://mongodb:27017");
var database = mongoClient.GetDatabase("product_db");
var productsCollection = database.GetCollection<Product>("products");

// Dependency Injection
builder.Services.AddSingleton(productsCollection);
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tüm ürünleri getir
app.MapGet("/products", async (IProductRepository repo) =>
{
    var products = await repo.GetAllAsync();
    return Results.Ok(products);
});

// Tek ürün getir
app.MapGet("/products/{id}", async (string id, IProductRepository repo) =>
{
    var product = await repo.GetByIdAsync(id);
    if (product == null) return Results.NotFound("Ürün bulunamadı.");
    return Results.Ok(product);
});

// Ürün ekle
app.MapPost("/products", async (ProductRequest req, IProductRepository repo) =>
{
    var product = new Product
    {
        Name = req.Name,
        Price = req.Price,
        Stock = req.Stock,
        Category = req.Category
    };
    await repo.CreateAsync(product);
    return Results.Created($"/products/{product.Id}", product);
});

// Ürün güncelle
app.MapPut("/products/{id}", async (string id, ProductRequest req, IProductRepository repo) =>
{
    var product = new Product
    {
        Name = req.Name,
        Price = req.Price,
        Stock = req.Stock,
        Category = req.Category
    };
    var result = await repo.UpdateAsync(id, product);
    if (!result) return Results.NotFound("Ürün bulunamadı.");
    return Results.Ok("Ürün güncellendi.");
});

// Ürün sil
app.MapDelete("/products/{id}", async (string id, IProductRepository repo) =>
{
    var result = await repo.DeleteAsync(id);
    if (!result) return Results.NotFound("Ürün bulunamadı.");
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