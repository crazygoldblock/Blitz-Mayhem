using UnityEngine;

public class Upgrade : MonoBehaviour
{
    public int id = -1;
    float state = 0;

    private void Start()
    {
        ParticleSystem sys = transform.GetChild(0).GetComponent<ParticleSystem>();
        ParticleSystem.MainModule ma = sys.main;

        switch (id)
        {
            case 0:
                ma.startColor = new Color(0.8f, 0f, 0.38f);
                break;
            case 1:
                ma.startColor = new Color(0.68f, 0f, 1f);
                break;
            case 2:
                ma.startColor = new Color(1f, 0.91f, 0f);
                break;
            case 3:
                ma.startColor = new Color(1f, 0.38f, 0f);
                break;
        }
    }
    private void FixedUpdate()
    {
        //animace vylepšení

        float sc = Mathf.Sin(state * 0.1f);
        
        transform.localScale = new Vector3(0.4f + sc / 12, 0.4f + sc / 12, 1f);

        state += 1f;
    }
}
