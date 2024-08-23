using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// kód který by se musel psát dvakrát protože by byl použit jak v lokální tak v hře po síti je jen jednou zde
/// tato třída není nikde sama použita
/// rozšiřují ji třídy NetworkGame/NetworkGameManager.cs a LocalGame/LocalGameManager.cs
/// všechny funkce označené jako virtual se přepisují v NetworkGameManageru
/// protože je potřeba kromě například vytvoření vylepšení také poslat po síti ostatní hráčům info o tom že se tak stalo
/// 
/// pokud například hráč vystřelil tak se zavolá funkce BulletShoot ze statické proměnné generalInstance
/// která bude nastavená na NetworkGameManager nebo LocalGameManager podle toho která hra je spuštěná
/// díky tomu se vždy volá správné přepsání této funkce 
/// není poté potřeba všude psát podmínky jestli právě probíhá hra po síti nebo ne
/// </summary>
public abstract class GeneralGameManager : MonoBehaviour
{
    public static HashMap<int, Vector2> spawnPositions = new();
    public static HashMap<int, Vector2> upgradePositions = new();
    public static GeneralGameManager generalInstance;

    public static bool isNetwork = false;
    public static int lives = -1;
    public static int playersCount = -1;
    public static string[] playerNames;

    public GameObject playerPref;
    public GameObject bullet;
    public GameObject scoresParent;
    public GameObject scorePrefab;
    public GameObject upgradePrefab;
    public GameObject bulletParticle;

    public Sprite[] playerSprites;
    public Sprite[] upgradeSprites;

    public int[] scores;
    public bool[] alive;
    public GameObject[] objScores;
    public GameObject[] players;

    public GameObject currentUpgrade;
    
    private float lastScoreIncrease = -10f;
    protected int roundNumber = 1;
    protected int lastNetworkRoundNumber = 1;

    public void Init(bool spawnUpgrades) {
        scores =           new int[playersCount];
        alive =           new bool[playersCount];
        players =   new GameObject[playersCount];
        objScores = new GameObject[playersCount];

        for (int i = 0; i < playersCount; i++)
        {
            alive[i] = true;
        }
        CreateScores();

        if (spawnUpgrades)
            StartCoroutine(SpawnUpgrades());
    }
    private void CreateScores()
    {
        Color[] scoresColors = new Color[4];
        scoresColors[0] = new Color(0.318f, 0.725f, 1f);
        scoresColors[1] = new Color(1f, 0.459f, 0.459f);
        scoresColors[2] = new Color(1f, 0.945f, 0.278f);
        scoresColors[3] = new Color(0.271f, 1f, 0.271f);

        for (int i = 0; i < playersCount; i++)
        {
            GameObject sc = Instantiate(scorePrefab);

            sc.transform.SetParent(scoresParent.transform, false);
            sc.transform.GetChild(1).GetComponent<TMP_Text>().text = playerNames[i];

            sc.transform.GetChild(0).GetComponent<Image>().color = scoresColors[i];

            sc.transform.GetChild(3).GetComponent<Image>().sprite = playerSprites[i];

            objScores[i] = sc;
        }
    }
    protected GameObject CreatePlayer(GameObject prefab, PlayerKeyBinds binds, int id)
    {
        GameObject pl = Instantiate(prefab);
        Sprite sp = playerSprites[id];

        pl.transform.position = spawnPositions.Get(id);
        pl.GetComponent<Controler>().SetValues(binds, id);
        pl.GetComponent<SpriteRenderer>().sprite = sp;

        return pl;
    }
    protected virtual void ResetRound()
    {
        for (int i = 0; i < playersCount; i++)
        {
            players[i].transform.position = spawnPositions.Get(i);
            alive[i] = true;
            players[i].SetActive(true);
        }
        DestroyUpgrade();

        foreach(GameObject pl in players)
        {
            Controler con = pl.GetComponent<Controler>();

            if (con != null)
                con.DisableAllUpgrades();
        }
        CameraMove.startRoundTime = Time.time;
    }
    public virtual void PlayerDeath(int playerId)
    {
        players[playerId].SetActive(false);
        alive[playerId] = false;

        int nAlive = 0;

        for (int i = 0; i < playersCount; i++)
        {
            if (alive[i])
                nAlive++;
        }

        if (nAlive != 1)
            return;

        for (int i = 0; i < playersCount; i++)
        {
            if (alive[i])
            {
                IncreaseScore(i);
                break;
            }
        }
        ResetRound();
    }
    protected void UpdateScores()
    {
        for (int i = 0; i < playersCount; i++)
        {
            objScores[i].transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = "SKÓRE: " + scores[i].ToString();
        }
    }
    private void IncreaseScore(int id)
    {
        if (Time.time - lastScoreIncrease < 1f)
            return;

        lastScoreIncrease = Time.time;

        if (lastNetworkRoundNumber > roundNumber)
        {
            roundNumber = lastNetworkRoundNumber;
            return;
        }
        roundNumber++;
        scores[id]++;

        if (scores[id] >= lives)
        {
            EndScreen.LoadEndScreen(playerNames, scores);
        }
        UpdateScores();
    }
    
    public virtual void ShootBullet(Vector2 pos, float direction, float force)
    {
        // funkce pro vystřelení je zde
        // protože je potřeba ji přepsat v NetworkManageru aby se také poslalo info posíti že se tak stalo

        GameObject b = Instantiate(bullet);

        b.transform.position = pos + new Vector2(direction * 0.7f, 0.13f);
        b.GetComponent<Bullet>().direction = direction;
        b.GetComponent<Bullet>().pushForce = force;
    }
    public virtual UpgradeSpawnData SpawnUpgrade(int id, int indexPos) 
    {
        DestroyUpgrade();

        if (indexPos == -1)
            indexPos = Random.Range(0, upgradePositions.Count);
        if (id == -1)
            id = Random.Range(0, 4);

        Vector2 pos = upgradePositions.Get(indexPos);
        GameObject u = Instantiate(upgradePrefab);

        currentUpgrade = u;
        
        u.transform.position = pos;
        u.GetComponent<Upgrade>().id = id;
        u.GetComponent<SpriteRenderer>().sprite = upgradeSprites[id];

        return new UpgradeSpawnData(indexPos, id);
    }
    public virtual void PickupUpgrade() 
    {
        DestroyUpgrade();
    }
    protected void DestroyUpgrade()
    {
        if (currentUpgrade != null)
        {
            Destroy(currentUpgrade);
            currentUpgrade = null;
        }
    }
    IEnumerator SpawnUpgrades()
    {
        while (true)
        {
            yield return new WaitForSeconds(Constants.TIME_BETWEEN_UPGRADES);
            generalInstance.SpawnUpgrade(-1, -1);
        }
    }
}
