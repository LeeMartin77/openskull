namespace OpenSkull.Api.Tests;


[TestClass]
public class TestTests
{
    private class ResultDummy {
    public static Result<string> GetStringResult(string input) {
        return Result.Success(input);
    }
    }

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

    [TestMethod]
    public void TestResult() {
        var result = ResultDummy.GetStringResult("MyTestString");
        result.Tap((string resultString) => Assert.AreEqual("MyTestString", resultString))
            .OnFailure(() => Assert.Fail());
    }
}