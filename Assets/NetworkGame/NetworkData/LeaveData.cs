
[System.Serializable]
public class LeaveData : NetworkData
{
    private string name;
    public LeaveData(string name) : base(Constants.LEAVE_ID)
    {
        this.name = name;
    }
    public string GetName()
    {
        return name;
    }
    public override string ToString()
    {
        return "Leave name: " + name;
    }
}
