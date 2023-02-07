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
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_dragged)
        {
            if (transform.parent == null)
            {
                transform.position = TimeIndependentLerp(transform.position, m_origin, 0.1f, Time.deltaTime);

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

    Vector3 TimeIndependentLerp(Vector3 _base, Vector3 _target, float _timeTo90, float _dt)
    {
        float lambda = -Mathf.Log(1.0f - 0.9f) / _timeTo90;
	    return Vector3.Lerp(_base, _target, 1 - Mathf.Exp(-lambda * _dt));
    }

    bool m_dragged = false;
    Vector3 m_origin;
    Transform m_originParent;
    Gigote m_gigote;
}
