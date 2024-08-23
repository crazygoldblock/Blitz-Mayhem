using System;

[Serializable]
public class DeathData : NetworkData
{
    private int playerId;
    private int roundNumber;
    public DeathData(int playerId, int roundNumber) : base(Constants.DEATH_ID)
    {
        this.playerId = playerId;
        this.roundNumber = roundNumber;
    }
    public int GetPlayerId()
    {
        return playerId;
    }
    public int GetRoundNumber()
    {
        return roundNumber;
    }
    public override string ToString()
    {
        return "death:" + playerId;
    }
}
