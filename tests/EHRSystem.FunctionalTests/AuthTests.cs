using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using EHRSystem.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private IWebDriver _driver;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _driver = new ChromeDriver();
    }

    [Fact]
    public void AdminLogin_ShowsAdminPanel()
    {
        // Arrange
        _driver.Navigate().GoToUrl("http://localhost:5000/Account/Login");
        
        // Act
        _driver.FindElement(By.Id("Email")).SendKeys("admin@ehr.com");
        _driver.FindElement(By.Id("Password")).SendKeys("Admin123!");
        _driver.FindElement(By.TagName("form")).Submit();

        // Assert
        Assert.Contains("Admin Panel", _driver.PageSource);
        Assert.DoesNotContain("Login", _driver.PageSource);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
} 