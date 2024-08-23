using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// používá se když se hráč připojí do hry po síti a čeká na ostatní hráče a na zahájení hry
///
/// host = hráč který hru založil
/// client = všichni ostatní hráči
/// </summary>
public class CreateGameClient : MonoBehaviour
{
    private static ServerInfo info;
    private static string playerName;

    private TcpMulticastClient tcpMulticast;
    
    private CreateNetworkGame net;
    void Start()
    {
        if (CreateNetworkGame.isHost)
        {
            enabled = false;
            return;
        }

        tcpMulticast = new(info.GetSenderAddr(), info.port);
        tcpMulticast.SendPacket(new JoinServerData(playerName));

        net = GetComponent<CreateNetworkGame>();
        net.host.SetActive(false);
    }
    private void Update()
    {
        for (NetworkData p = tcpMulticast.GetPacket(); p != null; p = tcpMulticast.GetPacket()) 
        {
            ParsePackets(p);
        }
    }
    private void ParsePackets(NetworkData packet)
    {
        // pakety od hosta

        switch (packet.GetId())
        {
            case Constants.START_GAME_ID:

                string[] names = ((StartGameData)packet).GetNames();
                int id = -1;

                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == playerName)
                    {
                        id = i;
                        break;
                    }
                }
                info.playerCount = names.Length;
                info.playerNames = names;

                NetworkGameManager.NewNetworkGame(
                    info,
                    false,
                    playerName,
                    id,
                    tcpMulticast
                );
                break;
            case Constants.PLAYER_NAMES_ID:
                CreatePlayerBanners(((PlayerNamesData)packet).GetNames());
                break;
            case Constants.PING_ID:
                if (((PingData)packet).GetCode() == Constants.SERVER_CLOSE_CODE)
                    SceneManager.LoadScene("Menu");
                break;
        }
    }
    private void CreatePlayerBanners(string[] players)
    {
        foreach (Transform child in net.playersObj.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            net.CreatePlayerBanner(i, players[i], net.playersObj.transform);
        }
    }
    public void Close()
    {
        // tlačítko zpět

        tcpMulticast.SendPacket(new LeaveData(playerName));
        SceneManager.LoadScene("Menu");
    }
    public static void JoinServer(ServerInfo info, string name)
    {
        // spuštění scény kde hráč čeká na zahájení hry

        CreateGameClient.info = info;

        playerName = name;
        CreateNetworkGame.isHost = false;

        SceneManager.LoadScene("CreateGame");
    }
}
