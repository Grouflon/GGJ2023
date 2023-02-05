using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Rules
{
    public string introSentence;
}

public class Customer : MonoBehaviour
{
    public int id;
    public Animal animal;
    public Transform customer;
    public Rules[] rules;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
