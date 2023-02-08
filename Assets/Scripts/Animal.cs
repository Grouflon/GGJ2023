using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public Transform nude;
    public Transform cooked;
    public ParticleSystem fireBurst;
    public AudioSource fireSound;

    public Fur[] GetFur()
    {
        return GetComponentsInChildren<Fur>(true); 
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Cook()
    {
        nude.gameObject.SetActive(false);
        cooked.gameObject.SetActive(true);

        if (!m_hasPlayedBurst)
        {
            fireBurst.Play();
            m_hasPlayedBurst = true;
            fireSound.Play();
        }
    }

    bool m_hasPlayedBurst = false;
}
