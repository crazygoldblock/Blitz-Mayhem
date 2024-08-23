using UnityEngine;

/// <summary>
/// pozice kde se může hráč objevit na začátku kola
/// </summary>
public class SpawnPos : MonoBehaviour
{
    public int id = -1;
    void Awake()
    {
        GeneralGameManager.spawnPositions.Add(id, transform.position);
    }
}
