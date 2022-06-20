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

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]

    public void ErrorPath_GivenInvalidNumberOfPlayers_ErrorsAppropriately(int playerCount)
    {
        Guid[] TestPlayerIds = new Guid[playerCount];
        for (int i = 0; i < playerCount; i++) {
          TestPlayerIds[i] = new Guid();
        }
        var GameResult = GameFunctions.CreateNew(TestPlayerIds);
        Assert.IsTrue(GameResult.IsFailure);
        GameResult.OnFailure((err) => Assert.AreEqual(GameCreationError.InvalidNumberOfPlayers, err));
    }
}