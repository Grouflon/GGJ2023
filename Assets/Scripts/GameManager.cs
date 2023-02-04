using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum GameState
{
    Cleaning,
    Drying,
    Pimping
}

public class GameManager : MonoBehaviour
{
    T GetObjectUnderMouse<T>()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, Vector2.zero);
        foreach(RaycastHit2D hit in hits)
        {
            T comp = hit.transform.GetComponent<T>();
            if (comp != null)
            {
                return comp;
            }
        }
        return default(T);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Prop prop = GetObjectUnderMouse<Prop>();
            if (prop != null)
            {
                onPropDragStart(prop);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_draggedProp != null)
            {
                onPropDragStop();
            }
        }

          // DRAG
        if (m_draggedProp != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane p = new Plane(new Vector3(0.0f, 0.0f, -1.0f), Vector3.zero);
            float enter = 0.0f;
            bool result = p.Raycast(ray, out enter);
            Assert.IsTrue(result);
            Vector3 point = ray.GetPoint(enter);

            m_draggedProp.transform.position = point + m_draggedPropOffset;
            m_draggedPropOffset = Vector3.Lerp(m_draggedPropOffset, Vector3.zero, 0.05f);
        }
    }

    void onPropDragStart(Prop _prop)
    {
        Assert.IsNotNull(_prop);
        Assert.IsNull(m_draggedProp);

        m_draggedProp = _prop;
        m_draggedProp.OnStartDrag();
        m_draggedProp.transform.SetParent(null);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(new Vector3(0.0f, 0.0f, -1.0f), Vector3.zero);
        float enter = 0.0f;
        bool result = p.Raycast(ray, out enter);
        Assert.IsTrue(result);
        Vector3 point = ray.GetPoint(enter);

        m_draggedPropOffset = m_draggedProp.transform.position - point;

    }

    void onPropDragStop()
    {
        Assert.IsNotNull(m_draggedProp);

        Animal targetAnimal = GetObjectUnderMouse<Animal>();
        m_draggedProp.OnStopDrag(targetAnimal);

        m_draggedProp = null;
    }


    Prop m_draggedProp;
    Vector3 m_draggedPropOffset;
}
