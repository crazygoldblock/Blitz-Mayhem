using System;
using UnityEngine;

[Serializable]
public class BulletData : NetworkData
{
    private float x;
    private float y;
    private float direction;
    private float force;

    public BulletData(Vector2 position, float direction, float force) : base(Constants.BULLET_ID)
    {
        this.direction = direction;
        this.force = force;
        x = position.x;
        y = position.y;
    }
    public float GetForce()
    {
        return force;
    }
    public Vector2 GetPos()
    {
        return new Vector2(x, y);
    }
    public float GetDirection()
    {
        return direction;
    }
    public override string ToString()
    {
        return "Bullet: " + GetPos() + ", " + direction;
    }
}
