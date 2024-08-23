using System;

[Serializable]
public class PlayerNamesData : NetworkData
{
    string[] names;
    public PlayerNamesData(string[] names) : base(Constants.PLAYER_NAMES_ID)
    {
        this.names = names;
    }
    public string[] GetNames()
    {
        return names;
    }
}
