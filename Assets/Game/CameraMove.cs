using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Pohyb kamery při samotné hře
/// pohybu s kamerou tak aby byly všichni živý hráči vždy vidět
/// </summary>
public class CameraMove : MonoBehaviour
{
    // seznam hráčů
    public static List<Transform> targets;
    public static float startRoundTime = 0;

    public float maxZoom = 40f;
    public float minZoom = 10f;
    public float zoomLimiter = 50f;

    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();

        startRoundTime = Time.time;
    }
    void LateUpdate()
    {
        if (targets == null || targets.Count == 0)
            return;

        // nehýbat s kamerou na začátku kola
        if (Time.time - startRoundTime < 0.8f)
            return;
        // pohyb kamry
        Move();
        // přiblížení/oddálení kamery podle vzdálenosti hráčů mezi sebou
        Zoom();
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance().y / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);

        float newZoom2 = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance().x / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom2, Time.deltaTime);
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = (centerPoint - transform.position) / 2 + transform.position;

        newPosition.z = -10;

        transform.position = newPosition;
    }

    Vector2 GetGreatestDistance()
    {
        Bounds bounds = new Bounds(new Vector2(-16f, 8.5f), Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            // přidat hráče jen pokud je naživu
            if (targets[i].gameObject.activeSelf)
                bounds.Encapsulate(targets[i].position);
        }
        return bounds.size;
    }

    Vector3 GetCenterPoint()
    {
        Bounds bounds = new Bounds(new Vector2(-16f, 8.5f), Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].gameObject.activeSelf)
            {
                Vector2 pos = CLampPosition(targets[i].position);
                bounds.Encapsulate(pos);
            }
        }
        return bounds.center;
    }
    private Vector2 CLampPosition(Vector2 pos)
    {
        pos.x = Mathf.Max(-26f, pos.x);
        pos.x = Mathf.Min(-5f, pos.x);
        
        pos.y = Mathf.Max(-6f, pos.y);
        pos.y = Mathf.Min(13f, pos.y);

        return pos;
    }
}
