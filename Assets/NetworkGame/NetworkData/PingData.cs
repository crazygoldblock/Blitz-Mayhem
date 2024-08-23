using System;

[Serializable]
public class PingData : NetworkData
{
    private int code;
    public PingData(int code) : base(5)
    {
        this.code = code;
    }
    public int GetCode()
    {
        return code;
    }
    public override string ToString()
    {
        return "Ping code: " + GetCode();
    }
}
