using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu : MonoBehaviour
{
    public List<GameObject> tabs;

    public GameObject serverPrefab;
    public GameObject serverList;
    public Slider nPlayers;
    public Slider rounds;
    public TMP_Dropdown resolution;
    public Toggle network;
    public Toggle fullscreenToggle;
    public TMP_InputField playerNameFieldClient;
    public TMP_InputField playerNameFieldHost;
    public Toggle limitFps;
    public TMP_Dropdown quality;

    List<GameObject> prefabs = new List<GameObject>();

    Resolution[] resolutions;

    public GameObject[] playerNames;
    
    private ServerInfo[] lastServers = null;
    private void Start()
    {
        UpdateOptions();
        SwitchTab(0);

        GetResolutions();

        StartCoroutine(ServerSearching());
    }
    void UpdateOptions()
    {
        tabs.Clear();    

        foreach (Transform child in GameObject.Find("Canvas").transform)
        {
            tabs.Add(child.gameObject);
        }
    }
    private void GetResolutions()
    {
        resolutions = new Resolution[4];

        Resolution res0 = new()
        {
            width = 1920,
            height = 1080
        };
        resolutions[0] = res0;

        Resolution res1 = new()
        {
            width = 1600,
            height = 900
        };
        resolutions[1] = res1;

        Resolution res2 = new()
        {
            width = 1280,
            height = 720
        };
        resolutions[2] = res2;

        Resolution res3 = new()
        {
            width = 1024,
            height = 576
        };
        resolutions[3] = res3;

        List<string> strRes = new();

        foreach(Resolution res in resolutions)
        {
            strRes.Add(res.width.ToString() + " x " + res.height.ToString());
        }
        resolution.ClearOptions();
        resolution.AddOptions(strRes);

        OptionsSave opt = SaveSystem.LoadJson<OptionsSave>("options");

        if (opt != null)
        {
            resolution.value = opt.resolutionIndex;
            resolution.RefreshShownValue();

            Screen.fullScreen = opt.fullscreen;
            fullscreenToggle.isOn = opt.fullscreen;

            quality.value = opt.quality;
            quality.RefreshShownValue();
            SetQuality(opt.quality);

            limitFps.isOn = opt.limitFps;
            LimitFps(opt.limitFps);
        }
        else
        {
            SaveSystem.SaveJson(new OptionsSave(false, 2, 1, false), "options");

            resolution.value = 2;
            resolution.RefreshShownValue();

            Screen.fullScreen = false;
        }
    }
    
    /// <summary>
    /// změnení okna v menu
    /// </summary>
    public void SwitchTab(int id)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (i == id)
                tabs[i].SetActive(true);
            else
                tabs[i].SetActive(false);
        }
    }
    /// <summary>
    /// jednou za sekundu hledání serverů na síti 
    /// </summary>
    IEnumerator ServerSearching()
    {        
        while (true)
        {
            yield return new WaitForSeconds(1f);
            RefreshServers(SearchForServers());
        }
    }
    /// <summary>
    /// obnovení zobrazení serverů na síti 
    /// </summary>
    private void RefreshServers(ServerInfo[] servers)
    {
        // obnovit servery jen pokud je jejich stav jiný
        if (lastServers != null && CompareServerLists(servers, lastServers))
            return;

        lastServers = servers;

        foreach (GameObject pref in prefabs)
        {
            Destroy(pref);
        }

        prefabs.Clear();

        foreach (ServerInfo server in servers)
        {
            if (server.started)
                continue;

            GameObject pref = Instantiate(serverPrefab, serverList.transform);

            prefabs.Add(pref);

            Button button = pref.transform.GetChild(2).gameObject.GetComponent<Button>();

            button.onClick.AddListener(delegate { JoinServer(server); });

            pref.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = server.hostPlayerName;
            pref.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text =
                "HRÁČI: " + server.playerCount + "/" + server.maxPlayers + "\n" +
                "POČET BODŮ NA VÝHRU: " + server.winRounds;
        }
    }
    private bool CompareServerLists(ServerInfo[] a, ServerInfo[] b) 
    {
        if (a.Length != b.Length)
            return false;

        foreach (ServerInfo infoA in a)
        {
            bool x = false;

            foreach (ServerInfo infoB in b)
            {
                if (infoA.ToString() == infoB.ToString())
                {
                    x = true;
                    break;
                }
            }

            if (!x)
                return false;
        }
        return true;
    }
    /// <summary>
    /// vyhledat servery na síti 
    /// </summary>
    public static ServerInfo[] SearchForServers()
    {
        List<ServerInfo> servers = new();
        UdpMulticastClient m = new(IPAddress.Parse(Constants.DISCOVERY_ADDR), Constants.MLUTICAST_PORT);

        for (int i = 0; i < 3; i++)
        {
            PingData dataping = new PingData(Constants.BROADCAST_CODE);

            m.SendPacket(dataping);

            Thread.Sleep(20);

            NetworkData data = m.GetPacket();

            while (data != null)
            {
                if (data.GetId() == Constants.SERVER_INFO_ID)
                    AddServer(servers, (ServerInfo)data);

                data = m.GetPacket();
            }
        }
        m.Close();

        if (DebugMode.DEBUG_NETWORK)
            Debug.Log("Nalezeno serverů: " + servers.Count);

        return servers.ToArray();
    }
    private static void AddServer(List<ServerInfo> servers, ServerInfo server)
    {
        foreach (ServerInfo s in servers)
        {
            if (s.multicastAddr.ToString().Equals(server.multicastAddr.ToString()))
                return;
        }
        servers.Add(server);
    }
    /// <summary>
    /// změnění zobrazené hodnoty při pohnutí slideru 
    /// </summary>
    public void PlayerCountChange(float count)
    {
        int n = (int)Math.Round(count, 0);
        UpdatePlayerNames(n);

        nPlayers.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "HRÁČI: " + n.ToString();
    }
    /// <summary>
    /// změnění zobrazené hodnoty při pohnutí slideru 
    /// </summary>
    public void WinRoundChange(float count)
    {
        int n = (int)Math.Round(count, 0);

        rounds.transform.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "BODY NA VÝHRU: " + n.ToString();
    }
    public void UpdatePlayerNamesNetworkButton()
    {
        UpdatePlayerNames( (int)Math.Round(nPlayers.value, 0) );
    }
    /// <summary>
    /// změnění počtu zobrazených polí pro zadávání jmén hráčů při vytváření hry 
    /// </summary>
    private void UpdatePlayerNames(int count)
    {
        int n = count;

        playerNames[0].SetActive(!network.isOn);
        playerNames[1].SetActive(n > 2 && !network.isOn);
        playerNames[2].SetActive(n > 3 && !network.isOn);
    }
    public static int StringToInt(string input)
    {
        try
        {
            return int.Parse(input);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    // ----------------------------- TLAČÍTKA ----------------------------------------
    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];

        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        OptionsSave opt = SaveSystem.LoadJson<OptionsSave>("options");
        opt.resolutionIndex = resolutionIndex;

        SaveSystem.SaveJson(opt, "options");
    }
    public void JoinServer(ServerInfo info)
    {
        if (DebugMode.DEBUG)
        {
            CreateGameClient.JoinServer(info, "Jakub");
            return;
        }
            
        string plName = playerNameFieldClient.text.Trim();

        if (info.maxPlayers == info.playerCount)
            return;

        if (plName == "")
        {
            StartCoroutine(WarnMissingName(playerNameFieldClient));
            return;
        }
            

        foreach (string str in info.playerNames)
        {
            if (str == plName)
                return;
        }

        CreateGameClient.JoinServer(info, plName);
    }
    /// <summary>
    /// vytvoření nové hry 
    /// </summary>
    public void CreateGame()
    {
        string plName = playerNameFieldHost.text.Trim();

        int playerCount = (int)Math.Round(nPlayers.value, 0);
        int lives = (int)Math.Round(rounds.value, 0);

        if (playerCount < 2)
            playerCount = 2;
        if (playerCount > 4)
            playerCount = 4;

        if (plName == "")
        {
            StartCoroutine(WarnMissingName(playerNameFieldHost));
            return;
        }

        List<string> names = new();

        names.Add(plName);

        if (network.isOn)
        {
            if (DebugMode.DEBUG)
            {
                CreateGameHost.StartCreatingGame("Michal", lives, playerCount);
                return;
            }
            
            CreateGameHost.StartCreatingGame(plName, lives, playerCount);
        }
        else
        {
            if (DebugMode.DEBUG)
            {
                LocalGameManager.StartGame(3, lives, new string[] { "Michal", "Jakub", "Adam" });
                return;
            }

            for (int i = 0; i < playerCount - 1; i++)
            {
                string n = playerNames[i].GetComponent<TMP_InputField>().text.Trim();
                // všechny jména hráčů musí být vyplněná
                if (n == "")
                {
                    StartCoroutine(WarnMissingName(playerNames[i].GetComponent<TMP_InputField>()));
                    return;
                }

                names.Add(n);
            }
            LocalGameManager.StartGame(playerCount, lives, names.ToArray());
        }
    }
    IEnumerator WarnMissingName(TMP_InputField o)
    {
        o.text = "Zadat jméno!";
        o.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().color = Color.red;

        yield return new WaitForSeconds(0.6f);
        
        o.text = "";
        o.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().color = new Color(1f, 0.78f, 0.34f);
    }
    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;

        OptionsSave opt = SaveSystem.LoadJson<OptionsSave>("options");

        opt.fullscreen = fullscreen;

        SaveSystem.SaveJson(opt, "options");
    }
    public void QuitApp()
    {
        Application.Quit();
    }
    public void SetQuality(int index)
    {
        switch (index)
        {
            case 0:
                QualitySettings.SetQualityLevel(0);
                break;
            case 1:
                QualitySettings.SetQualityLevel(2);
                break;
            case 2:
                QualitySettings.SetQualityLevel(4);
                break;
            }

        OptionsSave opt = SaveSystem.LoadJson<OptionsSave>("options");

        opt.quality = index;

        SaveSystem.SaveJson(opt, "options");
    }
    public void LimitFps(bool limit)
    {
        if (limit)
            Application.targetFrameRate = 60;
        else
            Application.targetFrameRate = -1;

        OptionsSave opt = SaveSystem.LoadJson<OptionsSave>("options");
        opt.limitFps = limit;

        SaveSystem.SaveJson(opt, "options");
    }
}

public class OptionsSave
{
    public bool fullscreen;
    public int resolutionIndex;
    public int quality;
    public bool limitFps;

    public OptionsSave(bool fullscreen, int res, int quality, bool limitFps)
    {
        this.fullscreen = fullscreen;
        resolutionIndex = res;
        this.quality = quality;
        this.limitFps = limitFps;
    }
}
