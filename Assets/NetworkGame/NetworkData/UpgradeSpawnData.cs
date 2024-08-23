
[System.Serializable]
public class UpgradeSpawnData : NetworkData
{
    private int indexPos;
    private int upgradeId;
    public UpgradeSpawnData(int indexPos, int upgradeId) : base(Constants.UPGRADE_SPAWN_ID)
    {
        this.indexPos = indexPos;
        this.upgradeId = upgradeId;
    }
    public int GetPos()
    {
        return indexPos;
    }
    public int GetUpgradeId()
    {
        return upgradeId;
    }
}
