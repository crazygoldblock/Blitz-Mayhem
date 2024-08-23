using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// zobrazení a spuštění konečného skóre na konci hry
/// </summary>
public class EndScreen : MonoBehaviour
{
    public GameObject pref;
    public GameObject parentObj;

    public Sprite[] playerSprites;

    private static string[] names;
    private static int[] scores;

    public static void LoadEndScreen(string[] names, int[] scores)
    {
        EndScreen.names = names;
        EndScreen.scores = scores;

        SceneManager.LoadScene("EndScreen");
    }
    public void Start()
    {
        int winIndex = -1;
        int score = -1;

        for (int i = 0; i < scores.Length; i++)
        {
            if (scores[i] > score)
            {
                winIndex = i;
                score = scores[i];
            }
        }
        CreateScore(winIndex);

        for (int i = 0; i < scores.Length; i++)
        {
            if (i == winIndex)
                continue;

            CreateScore(i);
        }
        StartCoroutine(LoadMenuAsync());
    }
    private void CreateScore(int id)
    {
        Color[] scoresColors = new Color[4];
        scoresColors[0] = new Color(0.318f, 0.725f, 1f);
        scoresColors[1] = new Color(1f, 0.459f, 0.459f);
        scoresColors[2] = new Color(1f, 0.945f, 0.278f);
        scoresColors[3] = new Color(0.271f, 1f, 0.271f);

        GameObject g = Instantiate(pref);
        g.transform.SetParent(parentObj.transform, false);

        g.transform.GetChild(0).GetComponent<Image>().color = scoresColors[id];
        g.transform.GetChild(1).GetComponent<TMP_Text>().text = names[id];
        g.transform.GetChild(2).GetComponent<TMP_Text>().text = "SKÓRE: " + scores[id].ToString();
        g.transform.GetChild(3).GetComponent<Image>().sprite = playerSprites[id];
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            LoadMenu();
    }
    IEnumerator LoadMenuAsync()
    {
        yield return new WaitForSeconds(8f);
        LoadMenu();
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
