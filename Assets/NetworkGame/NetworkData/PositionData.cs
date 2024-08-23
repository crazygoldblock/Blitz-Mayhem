using System;
using UnityEngine;

[Serializable]
public class PositionData : NetworkData
{
    private string name;
    private float x;
    private float y;
    private bool direction;
    private int seq;

    private static int staticSeq = 1;  
    public PositionData(string player, Vector2 position, bool direction) : base(Constants.POSITION_ID)
    {
        name = player;
        x = position.x;
        y = position.y;
        this.direction = direction;

        seq = staticSeq++;
    }
    public string GetPlayerName()
    {
        return name;
    }
    public Vector2 GetPos()
    {
        return new Vector2(x, y);
    }
    public bool GetDirection()
    {
        return direction;
    }
    public int GetSeq()
    {
        return seq;
    }
    public override string ToString()
    {
        return "Pos:" + name + ", " + x + " " + y;
    }
}
