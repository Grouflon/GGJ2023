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
        m_baseRotation = transform.rotation;
        rotation_biais = Random.Range(-1f, 1f);
        cleanliness = 0;
        dryness = 0;
        scale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = m_baserotation * Quaternion.Euler(0.0f, 0.0f, 0.0f*Time.timeSinceLevelLoad);
        float distance = Vector3.Distance(transform.position, windSource.transform.position);
        // damping
        transform.rotation = m_baseRotation * Quaternion.Euler(
            0.0f, 0.0f,
            windSource.amplitude * Mathf.Exp(- distance * distance * windSource.damping) * Mathf.Sin(
                windSource.temporal_frequency * Time.timeSinceLevelLoad
                - windSource.spatial_frequency * distance
                + rotation_biais * windSource.random
            )
        );

        //Debug.Log("C(" + cleanliness + ") - D(" + dryness + "");
 
        UpdataSprite();
    }

    void UpdataSprite()
    {
        int cleanIndex = GetCleanIndex();
        int drynessIndex = GetDryIndex();
        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        switch(cleanIndex){
            case 0:
                spriteRenderer.sprite = dirtySprite;
            break;
            case 1:
                switch(drynessIndex){
                    case 0:
                        spriteRenderer.sprite = cleanSprite;
                        //Debug.Log(cleanIndex);
                        break;
                    case 1:
                        spriteRenderer.sprite = drySprite;
                        break;
                    case 2:
                        spriteRenderer.sprite = toodrySprite;
                        break;
                }
            break;
            case 2:
                spriteRenderer.sprite = null;
                // Delete sprite
            break;
        }
    }



    public void IncrementClean(float intensity)
    {
        cleanliness += intensity;
    }
    public void IncrementDry(float intensity)
    {
        dryness += intensity;
    }

    public int GetCleanIndex()
    {
        return (int)(Mathf.Min(Mathf.Floor(cleanliness), 2));
    }
    public int GetDryIndex()
    {
        return (int)(Mathf.Min(Mathf.Floor(dryness), 2));
    }

    float cleanliness;
    float dryness;
    float scale;


    float rotation_biais;
    Quaternion m_baseRotation;
}
