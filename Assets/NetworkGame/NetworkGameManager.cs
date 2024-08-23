using System.Collections;
using System.Diagnostics;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hlavní třída pro hru po síti
/// V této třídě probíhá veškerá komunikace po síti
/// rozšiřuje třídu GeneralGameManager aby nebylo potřeba psát stejný kód drakrát pokud se použije i v lokální hře
/// také odpovídá na dotazy když jiný client hledá servery na síti
/// </summary>
public class NetworkGameManager : GeneralGameManager
{
    public static HashMap<string, LinearList<TimePositionPair>> positions = new();

    public static NetworkGameManager instance;
    public static UdpMulticastClient udpClient;
    public static TcpMulticastClient tcpClient;
    public static ServerInfo info;
    public static IPAddress serverAddr;

    public static bool isHost;
    public static string playerName;
    public static int playerId = -1;

    public HashMap<string, int> playerIds = new();
    public UdpMulticastClient discoveryClient;
    public Stopwatch watch = new();

    public GameObject enemyPrefab;
    public GameObject player;
    
    private int[] positionSeq;
    private int playersLoaded = 0;

    /// <summary>
    /// Volá se z menu pro start hry po síti
    /// načte scénu hry a předá všechny potřebná data o hře
    /// </summary>
    public static void NewNetworkGame(ServerInfo info, bool isHost, string playerName, int id, TcpMulticastClient tcpClient)
    {
        positions = new();

        NetworkGameManager.isHost = isHost;
        NetworkGameManager.playerName = playerName;
        GeneralGameManager.lives = info.winRounds;
        GeneralGameManager.playersCount = info.playerNames.Length;
        NetworkGameManager.info = info;
        NetworkGameManager.tcpClient = tcpClient;

        serverAddr = info.multicastAddr;
        playerNames = info.playerNames;
        playerId = id;

        udpClient = new(info.multicastAddr, Constants.MLUTICAST_PORT);
        
        SceneManager.LoadScene("NetworkGame");
    }
    void Start()
    {
        instance = this;
        generalInstance = this;
        isNetwork = true;
        
        discoveryClient = new(IPAddress.Parse(Constants.DISCOVERY_ADDR), Constants.MLUTICAST_PORT);
        positionSeq = new int[info.playerCount];
        CameraMove.targets = new();
        watch.Start();

        player = CreatePlayer(playerPref, KeyBindsManager.GetBinds()[0], playerId);
        player.SetActive(false);

        Init(isHost);
        
        for (int i = 0; i < playerNames.Length; i++)
        {
            playerIds.Add(playerNames[i], i);

            if (playerId != i)
                players[i] = CreateEnemy(enemyPrefab, spawnPositions.Get(i), playerNames[i], i);
            else
                players[i] = player;

            positions.Add(playerNames[i], new());

            CameraMove.targets.Add(players[i].transform);
        }

        if (isHost)
            info.started = true;

        // automaticky zavolat funkci Close() předtím než se aplikace zavře nebo změní scéna
        Application.quitting += Close;
        SceneManager.activeSceneChanged += CloseChange;

        CameraMove.startRoundTime = 999999f;

        if (!isHost)
            tcpClient.SendPacket(new PingData(Constants.GAME_LOADED_CODE));
    }
    private void Update()
    {
        if (isHost)
            ParsePacketsSearch();

        RecieveTcpPackets();
        RecieveUdpPackets();
    }
    GameObject CreateEnemy(GameObject prefab, Vector2 pos, string playerName, int id)
    {
        GameObject e = Instantiate(prefab);

        e.transform.position = pos;
        e.GetComponent<Enemy>().playerName = playerName;
        e.GetComponent<SpriteRenderer>().sprite = playerSprites[id];

        return e;
    }
    /// <summary>
    /// odpovídání na dotazy když jiný client hledá ostatní servery na síti
    /// </summary>
    private void ParsePacketsSearch()
    {
        for (NetworkData p = discoveryClient.GetPacket(); p != null; p = discoveryClient.GetPacket())
        {
            if (p.GetId() == Constants.PING_ID && ((PingData)p).GetCode() == Constants.BROADCAST_CODE)
                discoveryClient.SendPacket(info);
        }
    }
    public void SendChatMessage(string username, string text)
    {
        tcpClient.SendPacket(new ChatData(username, text));
    }
    /// <summary>
    /// přepsání funkce z GeneralManager trídy
    /// funkce se přepisují pokud je potřeba změnit jejich chování od lokální hry
    /// většinou přidání poslání packetu ostatním hráčům o dané akci
    /// </summary>
    public override void ShootBullet(Vector2 pos, float direction, float force)
    {
        base.ShootBullet(pos, direction, force);

        tcpClient.SendPacket(new BulletData(pos, direction, force));
    }
    public override void PlayerDeath(int playerId)
    {
        tcpClient.SendPacket(new DeathData(playerId, roundNumber));

        base.PlayerDeath(playerId);
    }
    public override UpgradeSpawnData SpawnUpgrade(int id, int indexPos) {
        UpgradeSpawnData data = base.SpawnUpgrade(id, indexPos);

        tcpClient.SendPacket(data);

        // hodnota která se vrátí z této funkce se nikdy nepoužíje
        // je tu jenom protože typ hodnoty která se vrací musí být stejná jako u funkce kterou přepisuje
        return null;
    }
    public override void PickupUpgrade() {
        base.PickupUpgrade();

        tcpClient.SendPacket(new UpgradePickupData(playerId));
    }
    protected override void ResetRound()
    {
        base.ResetRound();

        if (isHost)
            tcpClient.SendPacket(new RoundResetData(scores, roundNumber));
    }
    /// <summary>
    /// tato funkce se vždy zavolá když přijde ze sítě nový packet
    /// kromě packetů když se jeden z ostatních clientů hledá servery
    /// </summary>
    void ParseTcpPackets(NetworkData packet)
    {
        switch (packet.GetId())
        {
            case Constants.BULLET_ID:
                BulletData bulletData = (BulletData)packet;

                if (isHost)
                    ShootBullet(bulletData.GetPos(), bulletData.GetDirection(), bulletData.GetForce());
                else
                    base.ShootBullet(bulletData.GetPos(), bulletData.GetDirection(), bulletData.GetForce());

                break;

            case Constants.DEATH_ID:
                DeathData deathData = (DeathData)packet;

                if (deathData.GetRoundNumber() != roundNumber)
                    return;

                if (isHost)
                    PlayerDeath(deathData.GetPlayerId());
                else
                    base.PlayerDeath(deathData.GetPlayerId());

                break;

               

            case Constants.UPGRADE_PICKUP_ID:
                UpgradePickupData pickupData = (UpgradePickupData)packet;

                if (isHost)
                {
                    if (currentUpgrade != null)
                    {
                        tcpClient.SendPacket(new UpgradePickupData(pickupData.GetPlayerId()));
                    }
                }
                else
                {
                    if (pickupData.GetPlayerId() != playerId)
                        player.gameObject.GetComponent<Controler>().DisableAllUpgrades();
                }

                DestroyUpgrade();

                break;

            case Constants.UPGRADE_SPAWN_ID:
                UpgradeSpawnData spawnData = (UpgradeSpawnData)packet;

                base.SpawnUpgrade(spawnData.GetUpgradeId(), spawnData.GetPos());
                break;

            case Constants.CHAT_ID:
                ChatData chatData = (ChatData)packet;

                Chat.chat.SendMessageChat(chatData.GetUsername(), chatData.GetText(), true);

                if (isHost)
                    tcpClient.SendPacket(packet);

                break;

            case Constants.LEAVE_ID:
                if (isHost)
                    tcpClient.SendPacket(packet);

                SceneManager.LoadScene("Menu");

                break;
            case Constants.PING_ID:
                PingData pingData = (PingData)packet;

                if (pingData.GetCode() == Constants.GAME_LOADED_CODE)
                {
                    playersLoaded++;

                    if (playersLoaded >= playersCount - 1)
                    {
                        tcpClient.SendPacket(new PingData(Constants.START_GAME_CODE));
                        player.SetActive(true);
                        CameraMove.startRoundTime = Time.time;
                        StartCoroutine(SendPositions());
                    }       
                }
                if (pingData.GetCode() == Constants.START_GAME_CODE)
                {
                    player.SetActive(true);
                    CameraMove.startRoundTime = Time.time;
                    StartCoroutine(SendPositions());
                }
                break;
            case Constants.ROUND_RESET_ID:
                RoundResetData resetData = (RoundResetData)packet;

                if (!isHost)
                {
                    scores = resetData.GetSCores();
                    lastNetworkRoundNumber = resetData.GetRoundNumber();

                    UpdateScores();

                    base.ResetRound();
                }

                break;

            default:
                UnityEngine.Debug.LogError("Neznámí packet TCP: " + packet.GetId());
                break;
        }
    }
    /// <summary>
    /// přijímání pozic ostatních hráčů ze sítě
    /// </summary>
    void ParseUdpPackets(NetworkData packet) 
    {
        switch (packet.GetId())
        {
            case Constants.POSITION_ID:
                PositionData positionData = (PositionData)packet;

                int plId = playerIds.Get(positionData.GetPlayerName());

                if (positionData.GetSeq() <= positionSeq[plId])
                    break;
                else
                    positionSeq[plId] = positionData.GetSeq();

                TimePositionPair pair = new(positionData.GetPos(), watch.ElapsedTicks, positionData.GetDirection());
                LinearList<TimePositionPair> list = positions.Get(positionData.GetPlayerName());

                list.AddLast(pair);
                break;

            default:
                UnityEngine.Debug.LogError("Neznámí packet UDP: " + packet.GetId());
                break;
        }
    }
    IEnumerator SendPositions()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / Constants.TICK_RATE);
            udpClient.SendPacket(new PositionData(playerName, player.transform.position, Controler.instance.lastDirection == 1f));
        }
    }
    private void RecieveUdpPackets()
    {
        for (NetworkData p = udpClient.GetPacket(); p != null; p = udpClient.GetPacket())
        {
            ParseUdpPackets(p);
        }
    }
    private void RecieveTcpPackets()
    {
        for (NetworkData p = tcpClient.GetPacket(); p != null; p = tcpClient.GetPacket())
        {
            ParseTcpPackets(p);
        }
    }
    public void CloseGame()
    {
        tcpClient.SendPacket(new LeaveData(playerName));
    }
    public void Close()
    {
        try
        {
            StopCoroutine(SendPositions());

            tcpClient.SendPacket(new LeaveData(playerName));

            tcpClient.Close();
            udpClient.Close();
        }
        catch { }

        Application.quitting -= Close;
        SceneManager.activeSceneChanged -= CloseChange;
    }
    private void CloseChange(Scene current, Scene next)
    {
        if (current.name == null)
            return;

        Close();
    }
}
