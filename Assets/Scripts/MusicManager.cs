using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public Music musicPrefab;
    // Start is called before the first frame update
    void Start()
    {
        m_music = FindObjectOfType<Music>();
        PlayMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic()
    {
        if (m_music != null)
            return;

        m_music = Object.Instantiate(musicPrefab);
        m_music.audioSource.Play();
        DontDestroyOnLoad(m_music.gameObject);
    }

    Music m_music;

    public static MusicManager Get()
    {
        return FindObjectOfType<MusicManager>();
    }
}
