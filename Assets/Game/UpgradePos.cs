using UnityEngine;

/// <summary>
/// pozice kde se může objevit vylepšení
/// </summary>
public class UpgradePos : MonoBehaviour
{
    public int id = -1;
    void Awake()
    {
        GeneralGameManager.upgradePositions.Add(id, transform.position);
    }
}
