using System;
using System.Net;

[Serializable]
public class ServerInfo : NetworkData
{
    public IPAddress multicastAddr;
    public string hostPlayerName;
    public int playerCount;
    public int maxPlayers;
    public int winRounds;
    public bool started;
    public string[] playerNames;
    public int port = -1;

    public ServerInfo(IPAddress addr, string hostName, int plCount, int maxPl, int winRound, string[] names) : base(Constants.SERVER_INFO_ID)
    {
        multicastAddr = addr;
        hostPlayerName = hostName;
        playerCount = plCount;
        maxPlayers = maxPl;
        winRounds = winRound;
        started = false;
        playerNames = names;
    }
    public override string ToString()
    {
        return "Info: " + multicastAddr.ToString() + ", " + hostPlayerName + ", " + playerCount + ", " + winRounds;
    }
}
