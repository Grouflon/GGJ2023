using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
les scores sont testés en AND, à moins que min et max aient la même valeur, à ce moment là on ignore.
pour les props, n'importe quel tag positif valide le point, mais un seul tag négatif invalide forcément le point.
*/

[System.Serializable]
public struct Rules
{
    [Header("Text")]
    public string introSentence;

    [Header("Clean Score")]
    [Range(0.0f, 1.0f)]
    public float dirtyMin;
    [Range(0.0f, 1.0f)]
    public float dirtyMax;
    [Range(0.0f, 1.0f)]
    public float cleanMin;
    [Range(0.0f, 1.0f)]
    public float cleanMax;
    [Range(0.0f, 1.0f)]
    public float rippedMin;
    [Range(0.0f, 1.0f)]
    public float rippedMax;

    [Header("Dry Score")]
    [Range(0.0f, 1.0f)]
    public float wetMin;
    [Range(0.0f, 1.0f)]
    public float wetMax;
    [Range(0.0f, 1.0f)]
    public float dryMin;
    [Range(0.0f, 1.0f)]
    public float dryMax;
    [Range(0.0f, 1.0f)]
    public float burnedMin;
    [Range(0.0f, 1.0f)]
    public float burnedMax;

    [Header("Pimp Score")]
    public string[] positivePropsTags;
    public string[] negativePropsTags;
}

public class Customer : MonoBehaviour
{
    public int id;
    public Animal animal;
    public Transform customer;
    public Rules[] rules;

    public string[] failSentences;
    public string[] neutralSentences;
    public string[] successSentences;

    public AudioClip bark;
    public AudioClip animalBark;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
