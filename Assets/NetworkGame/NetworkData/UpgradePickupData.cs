using System;

[Serializable]
public class UpgradePickupData : NetworkData
{
    private int playerId;
    public UpgradePickupData(int playerId) : base(Constants.UPGRADE_PICKUP_ID)
    {
        this.playerId = playerId;
    }
    public int GetPlayerId()
    {
        return playerId;
    }
}
