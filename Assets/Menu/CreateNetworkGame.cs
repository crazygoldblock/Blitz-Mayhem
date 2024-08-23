using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// používá se při vytvíření hry po síti
///
/// host = hráč který hru založil
/// client = všichni ostatní hráči
/// </summary>
public class CreateNetworkGame : MonoBehaviour
{
    public static bool isHost = false;
    
    public GameObject playersObj;
    public GameObject infoObj;
    public GameObject client;
    public GameObject host;
    public GameObject playerPrefab;

    public Sprite[] playerSprites;
    
    public void BackButton()
    {
        // tlačítko zpět

        if (isHost)
            GetComponent<CreateGameHost>().Close();
        else
            GetComponent<CreateGameClient>().Close();
    }
    public GameObject CreatePlayerBanner(int id, string namePlayer, Transform parent)
    {
        Color[] scoresColors = new Color[4];
        scoresColors[0] = new Color(0.318f, 0.725f, 1f);
        scoresColors[1] = new Color(1f, 0.459f, 0.459f);
        scoresColors[2] = new Color(1f, 0.945f, 0.278f);
        scoresColors[3] = new Color(0.271f, 1f, 0.271f);

        GameObject sc = Instantiate(playerPrefab);

        sc.transform.SetParent(parent, false);

        sc.transform.GetChild(0).GetComponent<Image>().color = scoresColors[id];
        sc.transform.GetChild(1).GetComponent<TMP_Text>().text = namePlayer;

        Destroy(sc.transform.GetChild(2).gameObject);

        sc.transform.GetChild(3).GetComponent<Image>().sprite = playerSprites[id];

        return sc;
    }
}
