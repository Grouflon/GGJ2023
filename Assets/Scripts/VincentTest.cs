using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VincentTest : MonoBehaviour
{
    public WindSource windSource; 
    // Start is called before the first frame update
    void Start()
    {
        initFurTest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void initFurTest()
    {
        for(int i = 0; i <= 20; i++){
            for(int j = 0; j <= 20; j++){

                Fur tmp = Object.Instantiate(
                    fur_object,
                    new Vector3(0.2f*i, 0.2f*j, 0.0f),
                    Quaternion.Euler(0.0f, 0.0f, 0.0f));

                tmp.windSource = windSource;
            }
        }
    }

    public Fur fur_object;
}
