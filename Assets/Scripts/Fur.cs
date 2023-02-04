using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fur : MonoBehaviour
{
    public WindSource windSource; 
    // Start is called before the first frame update
    void Start()
    {
        m_origin = transform.position;
        m_baseRotation = transform.rotation;
        rotation_biais = Random.Range(-1f, 1f);
        cleanliness = 2;
        scale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = m_baserotation * Quaternion.Euler(0.0f, 0.0f, 0.0f*Time.timeSinceLevelLoad);
        transform.rotation = m_baseRotation * Quaternion.Euler(
            0.0f, 0.0f,
            windSource.amplitude * Mathf.Sin(
                windSource.spatial_frequency * Vector3.Distance(m_origin, windSource.transform.position)
                + windSource.temporal_frequency * Time.timeSinceLevelLoad
                + rotation_biais * windSource.random
            )
        );
    }

    void UpdataSprite()
    {
        int cleanIndex = (int)(Mathf.Max(Mathf.Round(cleanliness), 0));
        switch(cleanIndex){
            case -1:
            break;
            case 0:
            break;
            case 1:
            break;
            case 2:
            break;
        }
    }

    void IncrementClean(float intensity)
    {
        cleanliness -= intensity;
    }

    int GetScoreClean()
    {
        return (int)(Mathf.Max(Mathf.Round(cleanliness), 0));
    }

    float cleanliness;
    float scale;


    float rotation_biais;
    Quaternion m_baseRotation;
    Vector3 m_origin;
}
