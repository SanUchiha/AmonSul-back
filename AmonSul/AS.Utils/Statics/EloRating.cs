namespace AS.Utils.Statics;

public static class EloRating
{
    private const int KFactor = 32;

    public static int CalculateNewRating(int currentRating, int opponentRating, double score)
    {
        double expectedScore = GetExpectedScore(currentRating, opponentRating);
        return (int)(currentRating + KFactor * (score - expectedScore));
    }

    private static double GetExpectedScore(int playerRating, int opponentRating)
    {
        return 1.0 / (1.0 + Math.Pow(10, (opponentRating - playerRating) / 400.0));
    }
}