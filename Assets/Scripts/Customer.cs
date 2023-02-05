using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Rules
{
    [Header("Text")]
    public string introSentence;

    [Header("Score")]
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
