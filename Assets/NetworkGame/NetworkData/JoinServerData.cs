using System;

[Serializable]
public class JoinServerData : NetworkData
{
    private string playerName;
    public JoinServerData(string name) : base(4)
    {
        playerName = name;
    }
    public string GetName()
    {
        return playerName;
    }
    public override string ToString()
    {
        return "Join name: " + playerName; 
    }
}
