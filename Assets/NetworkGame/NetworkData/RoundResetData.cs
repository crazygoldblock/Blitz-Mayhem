using System;

[Serializable]
public class RoundResetData : NetworkData
{
    int[] scores;
    int roundNumber;

    public RoundResetData(int[] scores, int roundNumber) : base(Constants.ROUND_RESET_ID)
    {
        this.scores = scores;
        this.roundNumber = roundNumber;
    }
    public int[] GetSCores()
    {
        return scores;
    }
    public int GetRoundNumber()
    {
        return roundNumber;
    }
}
