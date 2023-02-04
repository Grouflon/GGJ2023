using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fur : MonoBehaviour
{
    public WindSource windSource; 
    // Start is called before the first frame update

    public Sprite cleanSprite;
    public Sprite drySprite;
    public Sprite toodrySprite;
    public Sprite dirtySprite;

    void Start()
    {
        m_origin = transform.position;
        m_baseRotation = transform.rotation;
        rotation_biais = Random.Range(-1f, 1f);
        cleanliness = -1;
        scale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = m_baserotation * Quaternion.Euler(0.0f, 0.0f, 0.0f*Time.timeSinceLevelLoad);
        float distance = Vector3.Distance(m_origin, windSource.transform.position);
        // damping
        transform.rotation = m_baseRotation * Quaternion.Euler(
            0.0f, 0.0f,
            windSource.amplitude * Mathf.Exp(- distance * distance * windSource.damping) * Mathf.Sin(
                windSource.temporal_frequency * Time.timeSinceLevelLoad
                - windSource.spatial_frequency * distance
                + rotation_biais * windSource.random
            )
        );

        UpdataSprite();
    }

    void UpdataSprite()
    {
        int cleanIndex = (int)(Mathf.Min(Mathf.Round(cleanliness), 1));
        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        Debug.Log(cleanIndex);
        switch(cleanIndex){
            case -1:
                Debug.Log("OKAY");
                spriteRenderer.sprite = dirtySprite;
            break;
            case 0:
                Debug.Log("Hm");
                spriteRenderer.sprite = cleanSprite;
            break;
            case 1:
                Debug.Log("Abd");
                // Delete sprite
            break;
        }
    }

    void IncrementClean(float intensity)
    {
        cleanliness += intensity;
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
