using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    public string[] tags;

    // Start is called before the first frame update
    void Start()
    {
        m_originParent = transform.parent;
        m_gigote = GetComponent<Gigote>();
        //ResetOrigin();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_dragged)
        {
            if (transform.parent == null)
            {
                transform.position = Vector3.Lerp(transform.position, m_origin, 0.1f);

                if (Vector3.Distance(transform.position, m_origin) < 0.1f)
                {
                    transform.position = m_origin;
                    transform.parent = m_originParent;
                }
            }
        }
    }

    public void OnStartDrag()
    {
        if (transform.parent == m_originParent)
        {
            m_origin = transform.position;
        }

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

    public void ResetOrigin()
    {
        m_origin = transform.position;
    }
    public void ForceReset()
    {
        transform.position = m_origin;
        transform.SetParent(m_originParent);
    }

    bool m_dragged = false;
    Vector3 m_origin;
    Transform m_originParent;
    Gigote m_gigote;
}
