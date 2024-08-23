using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    float state = 0;

    private void FixedUpdate()
    {
        float sc = Mathf.Sin(state * 0.06f);

        transform.localScale = new Vector3(0.9f + sc / 20, 0.9f + sc / 20, 1f);

        state += 1f;
    }
}
