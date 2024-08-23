using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool menuEnabled = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            SwitchPauseMenu();
        }
    }
    public void SwitchPauseMenu()
    {
        menuEnabled = !menuEnabled;
        pauseMenu.SetActive(menuEnabled);

        Controler.globalControl = !menuEnabled;
    }
    public void Continue()
    {
        SwitchPauseMenu();
    }
    public void Menu()
    {
        if (GeneralGameManager.isNetwork)
            NetworkGameManager.instance.CloseGame();

        SceneManager.LoadScene("Menu");
    }
}
