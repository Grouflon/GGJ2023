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

    public WindSource windSource;
    public float defaultDamping = 10.0f;
    public float defaultAmplitude = 2.0f;
    public float defaultTemporalFrequency = 2.0f;
    public float dryingWindDamping = 1.5f;
    public float dryingWindAmplitude = 8.0f;
    public float dryingWindTemporalFrequency = 20.0f;

    public float cleaningMaxVelocity = 60.0f;
    public float cleaningBrushStrength = 0.02f;

    public float dryingBrushRadius = 100.0f;
    public float dryingBrushDamping = 1.5f;
    public float dryingBrushStrength = 0.04f;

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

    Vector3 GetMouse3DPosition(float _z = -1.0f)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(new Vector3(0.0f, 0.0f, _z), Vector3.zero);
        float enter = 0.0f;
        bool result = p.Raycast(ray, out enter);
        Assert.IsTrue(result);
        return ray.GetPoint(enter);
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
        m_previousMousePostion = GetMouse3DPosition();
        m_windSourceOrigin = windSource.transform.position;
        foreach (Fur fur in m_currentAnimal.GetFur())
        {
            fur.windSource = windSource;
        }

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
        Vector3 mousePosition = GetMouse3DPosition();
        Vector3 velocity = (mousePosition - m_previousMousePostion) / Time.deltaTime;
        // Debug.Log(velocity.magnitude);

        bool windSourceLocked = false;
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {
                windSourceLocked = Input.GetMouseButton(0);

                if (windSourceLocked)
                {
                    foreach (Fur fur in m_currentAnimal.GetFur())
                    {
                        float speedFactor = Mathf.Clamp01(velocity.magnitude / cleaningMaxVelocity);
                        float distance = Vector3.Distance(mousePosition, fur.transform.position);
                        float increment = speedFactor * cleaningBrushStrength
                                        * Mathf.Exp(- Mathf.Pow(dryingBrushDamping * Mathf.Max(0, distance - dryingBrushRadius), 2));
                        // Debug.Log("Incr = " + speedFactor + " . " + cleaningBrushStrength + " - " + increment);
                        fur.IncrementClean(increment);
                    }
                }
            }
            break;
            case GameState.Drying:
            {
                windSourceLocked = Input.GetMouseButton(0);

                if (windSourceLocked)
                {
                    foreach (Fur fur in m_currentAnimal.GetFur())
                    {
                        float distance = Vector3.Distance(mousePosition, fur.transform.position);
                        float increment = dryingBrushStrength
                                        * Mathf.Exp(- Mathf.Pow(dryingBrushDamping * Mathf.Max(0, distance - dryingBrushRadius), 2));
                        // Debug.Log("Incr = " + dryingBrushStrength + " - " + increment);
                        fur.IncrementDry(increment);
                    }
                }
                
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
                    Vector3 point = GetMouse3DPosition();

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

        // WIND
        if (windSourceLocked)
        {
            Vector3 point = GetMouse3DPosition();

            if (!m_previousWindSourceLocked)
            {
                m_windSourceOffset = windSource.transform.position - point;
            }
            m_windSourceOffset = Vector3.Lerp(m_windSourceOffset, Vector3.zero, 0.05f);
            windSource.transform.position = point + m_windSourceOffset;
            //windSource.transform.position = Vector3.Lerp(windSource.transform.position, point, 0.05f);

            float lerpRatio = 0.1f;
            windSource.damping = Mathf.Lerp(windSource.damping, dryingWindDamping, lerpRatio);
            windSource.amplitude = Mathf.Lerp(windSource.amplitude, dryingWindAmplitude, lerpRatio);
            windSource.temporal_frequency = Mathf.Lerp(windSource.temporal_frequency, dryingWindTemporalFrequency, lerpRatio);
        }
        else
        {
            windSource.transform.position = Vector3.Lerp(windSource.transform.position, m_windSourceOrigin, 0.1f);

            float lerpRatio = 0.1f;
            windSource.damping = Mathf.Lerp(windSource.damping, defaultDamping, lerpRatio);
            windSource.amplitude = Mathf.Lerp(windSource.amplitude, defaultAmplitude, lerpRatio);
            windSource.temporal_frequency = Mathf.Lerp(windSource.temporal_frequency, defaultTemporalFrequency, lerpRatio);
        }
        m_previousWindSourceLocked = windSourceLocked;

        // UI
        timerText.text = m_currentTimer >= 0.0f ? Mathf.FloorToInt(m_currentTimer).ToString() : "Infinite";
        infoText.text = StateToText(m_currentstate);

        m_previousMousePostion = mousePosition;
    }

    void onPropDragStart(Prop _prop)
    {
        Assert.IsNotNull(_prop);
        Assert.IsNull(m_draggedProp);

        m_draggedProp = _prop;
        m_draggedProp.OnStartDrag();

        Vector3 point = GetMouse3DPosition();

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

    bool m_previousWindSourceLocked = false;
    Vector3 m_windSourceOrigin;
    Vector3 m_windSourceOffset;

    public Animal m_currentAnimal;

    float m_currentTimer = -1.0f;
    Vector3 m_previousMousePostion;
}
