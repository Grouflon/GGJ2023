using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public enum GameState
{
    None,
    Cleaning,
    Drying,
    Pimping,
    Transition
}

public class GameManager : MonoBehaviour
{
    public GameState overrideState = GameState.None;
    public float cleaningTime = 5.0f;
    public float dryingTime = 5.0f;
    public float pimpingTime = 5.0f;

    public TMP_Text timerText;
    public TMP_Text infoText;
    public TMP_Text titleText;

    public Animation titleAnimation;

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

    string StateToText(GameState _state)
    {
        string text = "";
        switch (_state)
        {
            case GameState.Cleaning: text = "Toilettage"; break;
            case GameState.Drying: text = "Séchage"; break;
            case GameState.Pimping : text = "Pimpage"; break;
        }
        return text;
    }

    void SetState(GameState _state)
    {
        // EXIT STATE
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {

            }
            break;
            case GameState.Drying:
            {

            }
            break;
            case GameState.Pimping:
            {
                if (m_draggedProp)
                {
                    onPropDragStop();
                }
            }
            break;
            case GameState.Transition:
            {
                infoText.enabled = true;
                timerText.enabled = true;
            }
            break;
            default:
            {

            }
            break;
        }

        m_currentstate = _state;

        // ENTER STATE
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {
                m_currentTimer = cleaningTime;
            }
            break;
            case GameState.Drying:
            {
                m_currentTimer = dryingTime;
            }
            break;
            case GameState.Pimping:
            {
                m_currentTimer = pimpingTime;
            }
            break;
            case GameState.Transition:
            {
                infoText.enabled = false;
                timerText.enabled = false;
                titleText.text = StateToText(m_nextState);
                titleAnimation.Play();
            }
            break;
            default:
            {

            }
            break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (overrideState != GameState.None)
        {
            SetState(overrideState);
            m_currentTimer = -1.0f; // infinite
        }
        else
        {
            m_nextState = GameState.Cleaning;
            SetState(GameState.Transition);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {
            }
            break;
            case GameState.Drying:
            {
            }
            break;
            case GameState.Pimping:
            {
                // INPUT
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
            break;
            case GameState.Transition:
            {
                if (!titleAnimation.isPlaying)
                {
                    SetState(m_nextState);
                    m_nextState = GameState.None;
                }
            }
            break;
            default:
            {

            }
            break;
        }

        // TIME
        if (m_currentTimer >= 0.0f)
        {
            m_currentTimer -= Time.deltaTime;

            if (m_currentTimer < 0.0f)
            {
                GameState nextState = GameState.None;
                switch (m_currentstate)
                {
                    case GameState.Cleaning: nextState = GameState.Drying; break;
                    case GameState.Drying: nextState = GameState.Pimping; break;
                }

                if (nextState != GameState.None)
                {
                    m_nextState = nextState;
                    SetState(GameState.Transition);
                }
            }
        }

        // UI
        timerText.text = m_currentTimer >= 0.0f ? Mathf.FloorToInt(m_currentTimer).ToString() : "Infinite";
        infoText.text = StateToText(m_currentstate);
    }

    void onPropDragStart(Prop _prop)
    {
        Assert.IsNotNull(_prop);
        Assert.IsNull(m_draggedProp);

        m_draggedProp = _prop;
        m_draggedProp.OnStartDrag();

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
    GameState m_currentstate;
    GameState m_nextState;

    float m_currentTimer = -1.0f;
}
