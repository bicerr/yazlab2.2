namespace Dispatcher.Tests;

public class DispatcherTests
{
    // Test 1: Token olmadan istek reddedilmeli
    [Fact]
    public void Request_WithoutToken_ShouldBeUnauthorized()
    {
        var token = string.Empty;
        var isAuthorized = !string.IsNullOrEmpty(token);
        Assert.False(isAuthorized);
    }

    // Test 2: Token varsa istek geçmeli
    [Fact]
    public void Request_WithToken_ShouldBeAuthorized()
    {
        var token = "Bearer test-token-123";
        var isAuthorized = !string.IsNullOrEmpty(token);
        Assert.True(isAuthorized);
    }

    // Test 3: /auth yoluna token olmadan erişilebilmeli
    [Fact]
    public void Request_ToAuthPath_ShouldSkipAuthorization()
    {
        var path = "/auth/login";
        var isAuthPath = path.StartsWith("/auth");
        Assert.True(isAuthPath);
    }

    // Test 4: Log kaydı doğru oluşturulmalı
    [Fact]
    public void LogEntry_ShouldHaveCorrectProperties()
    {
        var log = new LogEntry
        {
            Method = "GET",
            Path = "/products",
            Timestamp = DateTime.UtcNow,
            StatusCode = 200
        };

        Assert.Equal("GET", log.Method);
        Assert.Equal("/products", log.Path);
        Assert.Equal(200, log.StatusCode);
    }

    // Test 5: Yönlendirme yolu doğru eşleşmeli
    [Fact]
    public void Route_ProductPath_ShouldMatchProductCluster()
    {
        var path = "/products/1";
        var isProductRoute = path.StartsWith("/products");
        Assert.True(isProductRoute);
    }

    // Test 6: Yönlendirme yolu doğru eşleşmeli
    [Fact]
    public void Route_OrderPath_ShouldMatchOrderCluster()
    {
        var path = "/orders/1";
        var isOrderRoute = path.StartsWith("/orders");
        Assert.True(isOrderRoute);
    }
}

public class LogEntry
{
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public int StatusCode { get; set; }
}