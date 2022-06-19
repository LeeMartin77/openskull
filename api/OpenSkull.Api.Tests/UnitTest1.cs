namespace OpenSkull.Api.Tests;
using OpenSkull.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        WeatherForecast weather = new WeatherForecast {
            Date = new DateTime(),
            TemperatureC = 30,
            Summary = "Something"
        };

        Assert.AreEqual(weather.TemperatureF, 32 + (int)(30 / 0.5556));
    }
}