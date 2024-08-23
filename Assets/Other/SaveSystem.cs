using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static void AppendText(string data, string name)
    {
        string path = GetPath(name, "txt");

        File.AppendAllText(path, data);
    }
    public static void SaveText(string data, string name)
    {
        string path = GetPath(name, "txt");

        File.WriteAllText(path, data);
    }
    public static string LoadText(string name)
    {
        string path = GetPath(name, "txt");

        if (!File.Exists(path))
        {
            return "";
        }

        return File.ReadAllText(path);
    }
    public static void SaveKeyBinds(PlayerKeyBinds[] keybinds)
    {
        PlayerKeybindsData data = new PlayerKeybindsData();

        data.playerKeybinds = keybinds;

        SaveJson(data, "keybinds");
    }
    public static PlayerKeyBinds[] LoadKeyBinds()
    {
        PlayerKeybindsData data = LoadJson<PlayerKeybindsData>("keybinds");

        if (data == null)
            return null;

        return data.playerKeybinds;
    }
    public static void SaveJson<T>(T data, string name)
    {
        string json = JsonUtility.ToJson(data);
        string path = GetPath(name, "json");

        File.WriteAllText(path, json);
    }
    public static T LoadJson<T>(string name)
    {
        string path = GetPath(name, "json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            T data = JsonUtility.FromJson<T>(json);

            return data;
        }
        return default;
    }
    public static string GetPath(string name, string extension)
    {
        return Application.persistentDataPath + "/" + name + "." + extension;
    }
}
