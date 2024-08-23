using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// měnění klávesových zkratek v nastavení 
/// </summary>
[System.Serializable]
public class KeyBind
{
    public string nameOfTheKeybind;
    public KeyCode defaultBind;
    public Button buttonGameObject;
}
public class KeyBindsManager : MonoBehaviour
{
    [Header("KeyBinds")]
    public KeyBind[] keys;

    [Header("Messages")]
    [SerializeField]
    private string buttonAlreadyUsedMessage = "Already used!";
    [SerializeField]
    private string waitingForKeyMessage = "Waiting for key...";

    private int indexKeyToSet = -1;

    private void Start()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            int x = i;
            keys[i].buttonGameObject.onClick.AddListener(delegate { StartSettingKeyBind(x); });
        }
        LoadKeyBinds();
        SaveKeyBinds();
        UpdateButtonTexts();
    }
    private void Update()
    {
        ChangeBindOnClick();
    }
    IEnumerator KeyAlreadyUsed()
    {
        SetButtonText(keys[indexKeyToSet].buttonGameObject.gameObject, buttonAlreadyUsedMessage);
        yield return new WaitForSeconds(0.6f);
        SetButtonText(keys[indexKeyToSet].buttonGameObject.gameObject, waitingForKeyMessage);
    }
    public static void SetText(GameObject text, string message)
    {
        if (text != null)
        {
            if (text.GetComponent<TMP_Text>() != null)
                text.GetComponent<TMP_Text>().text = message;
            else if (text.GetComponent<Text>() != null)
                text.GetComponent<Text>().text = message;
        }
    }
    private void ChangeBindOnClick()
    {
        if (indexKeyToSet >= 0 && Input.anyKey)
        {
            KeyCode key = GetPressedKeys()[0];

            if (key == KeyCode.Escape || key == KeyCode.Return || key == KeyCode.Mouse0)
                return;

            if (CheckIfKeyCodeIsNotUsed(key, indexKeyToSet))
            {
                SetKey(indexKeyToSet, key);

                indexKeyToSet = -1;

                SaveKeyBinds();
            }
            else
            {
                StartCoroutine("KeyAlreadyUsed");
            }
        }
    }
    private bool CheckIfKeyCodeIsNotUsed(KeyCode keycode, int indexIgnore)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (i == indexIgnore)
                continue;

            if (keys[i].defaultBind == keycode)
                return false;
        }
        return true;
    }
    public void SaveKeyBinds()
    {
        PlayerKeyBinds[] binds = new PlayerKeyBinds[4];
        for (int i = 0; i < 4; i++)
        {
            binds[i] = new PlayerKeyBinds(
                keys[i * 4 + 0].defaultBind,
                keys[i * 4 + 1].defaultBind,
                keys[i * 4 + 2].defaultBind,
                keys[i * 4 + 3].defaultBind
            );
        }
        SaveSystem.SaveKeyBinds(binds);
    }
    public void LoadKeyBinds()
    {
        PlayerKeyBinds[] data = SaveSystem.LoadKeyBinds();

        if (data == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            SetKey(i * 4 + 0, data[i].left);
            SetKey(i * 4 + 1, data[i].right);
            SetKey(i * 4 + 2, data[i].jump);
            SetKey(i * 4 + 3, data[i].action);
        }
    }
    private void UpdateButtonTexts()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            SetKey(i, keys[i].defaultBind);
        }
    }

    private static KeyCode[] GetPressedKeys()
    {
        List<KeyCode> pressedKeysList = new List<KeyCode>();
        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(kcode))
                pressedKeysList.Add(kcode);
        }
        return pressedKeysList.ToArray();
    }
    public void SetKey(int index, KeyCode keycode)
    {
        keys[index].defaultBind = keycode;
        SetButtonText(keys[index].buttonGameObject.gameObject, keycode.ToString());
    }
    public static void SetButtonText(GameObject button, string text)
    {
        GameObject textObject = button.transform.GetChild(0).gameObject;
        SetText(textObject, text);
    }
    public void StartSettingKeyBind(int indexKeyToBind)
    {
        indexKeyToSet = indexKeyToBind;
        SetButtonText(keys[indexKeyToBind].buttonGameObject.gameObject, waitingForKeyMessage);
    }
    public static PlayerKeyBinds[] GetBinds()
    {
        PlayerKeyBinds[] loadedKeys = SaveSystem.LoadKeyBinds();

        return loadedKeys;
    }
}

[System.Serializable]
class PlayerKeybindsData
{
    public PlayerKeyBinds[] playerKeybinds;
}
