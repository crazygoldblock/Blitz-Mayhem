using System;
using UnityEngine;
/// <summary>
/// zapdnutí vypisování aktuálního stavu hry do konzole v Unity editoru
/// </summary>
public class DebugMode : MonoBehaviour
{
    public static bool DEBUG = false;
    public static bool DEBUG_NETWORK = false;

    public GameObject[] debugObjects;
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.UpArrow))
        {
            DEBUG = !DEBUG;

            foreach (GameObject go in debugObjects)
            {
                go.SetActive(DEBUG);
            } 
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            DEBUG_NETWORK = !DEBUG_NETWORK;
        }
    }
}
