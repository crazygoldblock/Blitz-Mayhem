using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// používá se když hráč vytvoří novou hru po síti a čeká na ostatní hráče
///
/// host = hráč který hru založil
/// client = všichni ostatní hráči
/// </summary>
public class CreateGameHost : MonoBehaviour 
{
    private static int lives;
    private static int playerCount;
    private static string playerName;

    private CreateNetworkGame net;
    private UdpMulticastClient clientSearch;
    private TcpMulticastClient tcpMulticast;

    private List<string> players;
    private IPAddress addr;
    private ServerInfo info;

    private int playerListCount;
    void Start()
    {
        if (!CreateNetworkGame.isHost)
        {
            enabled = false;
            return;
        }

        addr = GetAvailableAddr();

        if (DebugMode.DEBUG_NETWORK)
            Debug.Log("Multicast adresa: " + addr);

        net = GetComponent<CreateNetworkGame>();
        net.client.SetActive(false);

        net.infoObj.GetComponent<TMP_Text>().text =
            "Maximální počet hráců: " + playerCount + "\n" +
            "Počet bodů na výhru: " + lives + "\n";

        players = new();

        clientSearch = new(IPAddress.Parse(Constants.DISCOVERY_ADDR), Constants.MLUTICAST_PORT);
        tcpMulticast = new();
        
        players.Add(playerName);

        info = new(
            addr,
            playerName,
            playerListCount,
            playerCount,
            lives,
            players.ToArray()
        );

        info.port = tcpMulticast.GetPort();
    }
    public static void StartCreatingGame(string name, int lives, int playerCount)
    {
        CreateNetworkGame.isHost = true;

        CreateGameHost.playerName = name;
        CreateGameHost.lives = lives;
        CreateGameHost.playerCount = playerCount;

        SceneManager.LoadScene("CreateGame");
    }
    void Update()
    {
        if (players.Count != playerListCount)
        {
            playerListCount = players.Count;
            info.playerCount = playerListCount;
            info.playerNames = players.ToArray();

            CreatePlayerBanners();
        }
        ParsePackets();
        ParsePacketsSearch();
    }
    public void StartGame()
    {
        if (players.Count < playerCount)
            return;

        tcpMulticast.SendPacket(new StartGameData(players.ToArray()));
        
        clientSearch.Close();

        NetworkGameManager.NewNetworkGame(
            info,
            true,
            playerName,
            0,
            tcpMulticast
        );
    }
    public IPAddress GetAvailableAddr()
    {
        // získání multicast adresy/skupiny kterou ještě žádná hra nepoužívá

        ServerInfo[] servers = Menu.SearchForServers();

        int a = Constants.ADDR_START_NUMBER;

        IPAddress addr = IPAddress.Parse(Constants.MLUTICAST_ADDR + a.ToString());
        
        while (IsAddrPresent(servers, addr))
        {
            a++;
            addr = IPAddress.Parse(Constants.MLUTICAST_ADDR + a.ToString());
        }
        return addr;
    }
    private bool IsAddrPresent(ServerInfo[] servers, IPAddress addr)
    {
        foreach (ServerInfo server in servers)
        {
            if (server.multicastAddr.ToString() == addr.ToString())
                return true;
        }
        return false;
    }
    private void CreatePlayerBanners()
    {
        foreach (Transform child in net.playersObj.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count; i++) 
        {
            net.CreatePlayerBanner(i, players[i], net.playersObj.transform);
        }
        tcpMulticast.SendPacket(new PlayerNamesData(players.ToArray()));
    }
    void ParsePackets()
    {
        // pakety od clientů

        for (NetworkData p = tcpMulticast.GetPacket(); p != null; p = tcpMulticast.GetPacket())
        {
            switch (p.GetId())
            {
                case Constants.JOIN_ID:
                    players.Add(((JoinServerData)p).GetName());
                    break;

                case Constants.LEAVE_ID:
                    players.Remove(((LeaveData)p).GetName());
                    break;
            }
        }
    }
    void ParsePacketsSearch()
    {
        // odpovídá na dotazy které servery jsou na lokální síti

        for (NetworkData p = clientSearch.GetPacket(); p != null; p = clientSearch.GetPacket())
        {
            if (p.GetId() == Constants.PING_ID && ((PingData)p).GetCode() == Constants.BROADCAST_CODE)
                clientSearch.SendPacket(info);
        }
    }
    public void Close()
    {
        // tlačítko zpět

        tcpMulticast.SendPacket(new PingData(Constants.SERVER_CLOSE_CODE));

        tcpMulticast.Close();
        clientSearch.Close();

        SceneManager.LoadScene("Menu");
    }
    
}
