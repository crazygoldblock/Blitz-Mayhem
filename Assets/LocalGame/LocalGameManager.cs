using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hlavní třída pro lokální hru
/// rozšiřuje třídu GeneralGameManager aby nebylo potřeba psát stejný kód drakrát pokud se použije i ve hře posíti
/// </summary>
public class LocalGameManager : GeneralGameManager
{
    public static LocalGameManager instance;

    /// <summary>
    /// Volá se z menu pro start lokální hry 
    /// načte scénu hry a předá všechny potřebná data o hře
    /// </summary>
    public static void StartGame(int playerCount, int lives, string[] names) 
    {
        GeneralGameManager.playersCount = playerCount;
        GeneralGameManager.lives = lives;
        GeneralGameManager.playerNames = names;

        SceneManager.LoadScene("LocalGame");
    }
    void Start()
    {
        Init(true);

        instance = this;
        generalInstance = this;

        for (int i = 0; i < playersCount; i++)
        {
            players[i] = CreatePlayer(playerPref, KeyBindsManager.GetBinds()[i], i);
        }
        CameraMove.targets = new();

        foreach (GameObject player in players)
        {
            CameraMove.targets.Add(player.transform);
        }
    }
}
