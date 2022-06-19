using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;


[TestClass]
public class GameFunctionTests
{
    [TestMethod]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    public void HappyPath_GivenValidNumberOfPlayers_CreatesGame(int playerCount)
    {
        Guid[] TestPlayerIds = new Guid[playerCount];
        for (int i = 0; i < playerCount; i++) {
          TestPlayerIds[i] = new Guid();
        }
        var GameResult = GameFunctions.CreateNew(TestPlayerIds);
        Assert.IsTrue(GameResult.IsSuccess);
    }
}