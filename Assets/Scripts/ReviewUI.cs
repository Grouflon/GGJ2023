using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviewUI : MonoBehaviour
{
    public ReviewLine[] lines;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetReviews(Customer[] _customers, int[] _scores)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            lines[i].SetData(_customers[i], _scores[i]);
        }
    }
}
