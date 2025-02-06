using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net.Http.MultipartFormData;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using EHRSystem.Web;

public class RoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly UserManager<IdentityUser> _userManager;

    public RoleTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _userManager = factory.Services.GetRequiredService<UserManager<IdentityUser>>();
    }

    [Fact]
    public async Task ChangeRole_UpdatesUserRole()
    {
        // Arrange: Login as admin
        var authResponse = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", "admin@ehr.com"),
            new KeyValuePair<string, string>("Password", "Admin123!")
        }));
        
        // Act: Change role
        var formData = new MultipartFormDataContent
        {
            { new StringContent("user123"), "userId" },
            { new StringContent("Doctor"), "newRole" }
        };
        var response = await _client.PostAsync("/Account/ChangeRole", formData);

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await _userManager.FindByIdAsync("user123");
        Assert.Contains("Doctor", await _userManager.GetRolesAsync(user));
    }
} 