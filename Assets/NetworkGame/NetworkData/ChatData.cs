using System;

[Serializable]
public class ChatData : NetworkData
{
    private string username;
    private string text;

    public ChatData(string username, string text) : base(Constants.CHAT_ID)
    {
        this.username = username;
        this.text = text;
    }
    public string GetUsername()
    {
        return username;
    }
    public string GetText()
    {
        return text;
    }
}
