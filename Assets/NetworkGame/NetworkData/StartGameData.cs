
[System.Serializable]
public class StartGameData : NetworkData
{
    private string[] names;
    public StartGameData(string[] names) : base(Constants.START_GAME_ID)
    {
        this.names = names;
    }
    public string[] GetNames()
    {
        return names;
    }
}
