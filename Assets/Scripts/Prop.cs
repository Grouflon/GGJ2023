using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        m_origin = transform.position;
        m_gigote = GetComponent<Gigote>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_dragged)
        {
            if (transform.parent == null)
            {
                transform.position = Vector3.Lerp(transform.position, m_origin, 0.1f);
            }
        }
    }

    public void OnStartDrag()
    {
        m_dragged = true;
        transform.SetParent(null);
        m_gigote.on = false;

    }

    public void OnStopDrag(Animal _targetAnimal)
    {
        m_dragged = false;

        if (_targetAnimal != null)
        {
            transform.SetParent(_targetAnimal.transform);
        }
        m_gigote.on = true;
    }

    bool m_dragged = false;
    Vector3 m_origin;
    Gigote m_gigote;
}