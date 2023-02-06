using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEvents : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCustomerArrived()
    {
        FindObjectOfType<GameManager>().Bark();
    }

    void OnAnimalArrived()
    {
        //FindObjectOfType<GameManager>().AnimalBark();
    }
}
