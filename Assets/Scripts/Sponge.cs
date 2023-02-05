using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sponge : MonoBehaviour
{

    public ParticleSystem ParticleA;
    public ParticleSystem ParticleB;
    public float delta_time = 0.25f;
    public float intensity_threshold = 0.00001f;

    // Start is called before the first frame update
    void Start()
    {
        _livetime = 0f;
        StopBubble();
        GetComponent<SpriteRenderer>().material.renderQueue = 3500;
        ParticleA.GetComponent<SpriteRenderer>().material.renderQueue = 3499;
        ParticleB.GetComponent<SpriteRenderer>().material.renderQueue = 3499;
    }
    
    void Update()
    {
        _livetime -= Time.deltaTime;
        if (particle_is_on & _livetime <= 0f)
        {
            StopBubble();
        }
    }

    public void ModifyBubble(float intensity)
    {
        Debug.Log("> " + intensity + " (" + _livetime + ") - " + particle_is_on);
        if (intensity >= intensity_threshold)
            StartBubble();
    }

    void StartBubble()
    {
        if (!particle_is_on)
        {
            ParticleA.Play();
            ParticleB.Play();
            particle_is_on = true;
        }
        _livetime = delta_time;
    }
    void StopBubble()
    {
        ParticleA.Stop();
        ParticleB.Stop();
        particle_is_on = false;
    }

    bool particle_is_on;
    float _livetime;

}
