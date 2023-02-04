using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gigote : MonoBehaviour
{
    public float amplitude = 10.0f;
    public float period = 5.0f;
    public bool on = true;

    // Start is called before the first frame update
    void Start()
    {
        m_bias = Random.Range(0.0f, period);
        m_baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (on)
        {
            transform.rotation = m_baseRotation * Quaternion.Euler(0.0f, 0.0f, amplitude * Mathf.Sin(Mathf.PI * 2.0f * (Time.timeSinceLevelLoad + m_bias) / period));
        }
    }

    float m_bias = 0.0f;
    Quaternion m_baseRotation;
}
