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

        windSource = FindObjectOfType<WindSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = m_baserotation * Quaternion.Euler(0.0f, 0.0f, 0.0f*Time.timeSinceLevelLoad);
        /*Vector3 fur_position = transform.position;
        fur_position.z = 0f;
        Vector3 wind_position = windSource.transform.position;
        wind_position.z = 0f;
        float distance = Vector3.Distance(fur_position, wind_position);*/

        float distance = Mathf.Sqrt(
            Mathf.Pow(transform.position.x- windSource.transform.position.x, 2)
            + Mathf.Pow(transform.position.y- windSource.transform.position.y, 2));

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
        int cleanIndex = GetCleanIndex();
        int drynessIndex = GetDryIndex();
        SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        switch(cleanIndex){
            case 0:
                switch(drynessIndex){
                    case 0:
                        // Dirty and Wet
                        spriteRenderer.sprite = dirtySprite;
                        break;
                    case 1:
                        // Dirty and Dry
                        spriteRenderer.sprite = dirtySprite;
                        break;
                    case 2:
                        // Dirty and Burned
                        spriteRenderer.sprite = toodrySprite;
                        break;
                }
                
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
        if (cleanliness < 1)
            return 0;
        else if (cleanliness < nakedvalue)
            return 1;
        else
            return 2;
    }
    public int GetDryIndex()
    {
        if (dryness < 1)
            return 0;
        else if (dryness < burnedvalue)
            return 1;
        else
            return 2;
    }

    float cleanliness;
    float dryness;

    float nakedvalue = 4;
    float burnedvalue = 4;

    float rotation_biais;
    Quaternion m_baseRotation;
}
