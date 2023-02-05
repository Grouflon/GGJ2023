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
    Transition,
    Intro,
    Outro,
    End,
}

public class GameManager : MonoBehaviour
{
    [Header("Debug")]
    public GameState overrideState = GameState.None;

    [Header("Game Rules")]
    public float cleaningTime = 5.0f;
    public float dryingTime = 5.0f;
    public float pimpingTime = 5.0f;
    public Customer[] customers;

    [Header("Brush Rules")]
    public float cleaningMaxVelocity = 60.0f;
    public float cleaningBrushStrength = 0.02f;

    public float dryingBrushRadius = 100.0f;
    public float dryingBrushDamping = 1.5f;
    public float dryingBrushStrength = 0.04f;

    [Header("Wind Effect")]
    public float defaultDamping = 10.0f;
    public float defaultAmplitude = 2.0f;
    public float defaultTemporalFrequency = 2.0f;
    public float dryingWindDamping = 1.5f;
    public float dryingWindAmplitude = 8.0f;
    public float dryingWindTemporalFrequency = 20.0f;

    [Header("Internal")]
    public TMP_Text timerText;
    public TMP_Text infoText;
    public Animation titleAnimation;
    public WindSource windSource;
    public Transform sponge;
    public Transform dryer;
    public Animation scene;
    public Transform customerContainer;
    public Transform animalContainer;
    public RectTransform titleClean;
    public RectTransform titleDry;
    public RectTransform titlePimp;
    public RectTransform textContainer;
    public TMP_Text ayaText;
    public TMP_Text justineText;
    public TMP_Text racineText;
    public Transform drawerPrefab;
    public Transform drawerContainer;
    public AudioSource barkAudioSource;
    public AudioSource toolAudioSource;
    public AudioSource tictacAudioSource;
    public AudioClip spongeSound;
    public AudioClip dryerSound;
    public AudioClip pingAudio;
    
    // Start is called before the first frame update
    void Start()
    {
        m_previousMousePostion = GetMouse3DPosition();
        m_windSourceOrigin = windSource.transform.position;

        if (overrideState != GameState.None)
        {
            SetState(overrideState);
            m_currentTimer = -1.0f; // infinite
        }
        else
        {
            SetState(GameState.Intro);
        }
    }
    
    void SetState(GameState _state)
    {
        // EXIT STATE
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {
                sponge.gameObject.SetActive(false);
                Cursor.visible = true;
                
                toolAudioSource.Stop();

                float dirty, clean, ripped;
                dirty = clean = ripped = 0;
                get_percentage_cleanliness(ref dirty, ref clean, ref ripped);
            }
            break;
            case GameState.Drying:
            {
                dryer.gameObject.SetActive(false);
                Cursor.visible = true;

                toolAudioSource.Stop();

                float wet, dry, burned;
                burned = dry = wet = 0;
                get_percentage_dryness(ref wet, ref dry, ref burned);
            }
            break;
            case GameState.Pimping:
            {
                if (m_draggedProp)
                {
                    onPropDragStop();
                }
                m_isInDryEnd = false;
                m_waitForDryReset = false;
                ClearAllChildren(drawerContainer);

            }
            break;
            case GameState.Transition:
            {
                RectTransform title = StateToTitle(m_nextState);
                title.gameObject.SetActive(false);

                infoText.enabled = true;
                timerText.enabled = true;
            }
            break;
            case GameState.Intro:
            {
                textContainer.gameObject.SetActive(false);
                ayaText.gameObject.SetActive(false);
                justineText.gameObject.SetActive(false);
                racineText.gameObject.SetActive(false);
            }
            break;
            case GameState.Outro:
            {

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

                sponge.gameObject.SetActive(true);
                sponge.transform.position = GetMouse3DPosition();
                Cursor.visible = false;

                toolAudioSource.clip = spongeSound;
            }
            break;
            case GameState.Drying:
            {
                m_currentTimer = dryingTime;

                dryer.gameObject.SetActive(true);
                dryer.transform.position = GetMouse3DPosition();
                Cursor.visible = false;
                
                toolAudioSource.clip = dryerSound;
            }
            break;
            case GameState.Pimping:
            {
                m_currentTimer = pimpingTime;
                scene.Play("DrawerIn");
                m_waitForDryReset = true;

                ClearAllChildren(drawerContainer);
                Object.Instantiate(drawerPrefab, drawerContainer);
            }
            break;
            case GameState.Transition:
            {
                infoText.enabled = false;
                timerText.enabled = false;
                RectTransform title = StateToTitle(m_nextState);
                title.gameObject.SetActive(true);
                titleAnimation.Play();

                barkAudioSource.PlayOneShot(pingAudio);
            }
            break;
            case GameState.Intro:
            {
                Customer customerData = customers[m_currentCustomerIndex];
                m_currentRule = Random.Range(0, customerData.rules.Length); 

                ClearAllChildren(customerContainer);
                ClearAllChildren(animalContainer);

                Transform customer = Object.Instantiate(customerData.customer, customerContainer);
                customer.localPosition = Vector3.zero;

                m_currentAnimal = Object.Instantiate(customerData.animal, animalContainer);
                m_currentAnimal.transform.localPosition = Vector3.zero;

                foreach (Fur fur in m_currentAnimal.GetFur())
                {
                    fur.windSource = windSource;
                }

                scene.Play("CustomerEntry");
            }
            break;
            case GameState.Outro:
            {
                scene.Play("CustomerExit");
            }
            break;
            default:
            {

            }
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = GetMouse3DPosition();
        Vector3 velocity = (mousePosition - m_previousMousePostion) / Time.deltaTime;

        bool windSourceLocked = false;
        switch (m_currentstate)
        {
            case GameState.Cleaning:
            {
                windSourceLocked = Input.GetMouseButton(0);

                // AFFECT FUR
                if (windSourceLocked)
                {
                    if (!toolAudioSource.isPlaying)
                    {
                        toolAudioSource.Play();
                    }

                    float sum_increment = 0;
                    foreach (Fur fur in m_currentAnimal.GetFur())
                    {
                        float speedFactor = Mathf.Clamp01(velocity.magnitude / cleaningMaxVelocity);
                        float distance = Mathf.Sqrt(
                            Mathf.Pow(mousePosition.x- fur.transform.position.x, 2)
                            + Mathf.Pow(mousePosition.y- fur.transform.position.y, 2));
                        // float distance = Vector3.Distance(mousePosition, fur.transform.position);
                        float increment = speedFactor * cleaningBrushStrength
                                        * Mathf.Exp(- Mathf.Pow(dryingBrushDamping * Mathf.Max(0, distance - dryingBrushRadius), 2));
                        fur.IncrementClean(increment);
                        sum_increment += increment;
                    }
                    sponge.GetComponent<Sponge>().ModifyBubble(sum_increment);

                    toolAudioSource.volume = sum_increment * 0.4f;
                }
                else
                {
                    toolAudioSource.Stop();
                }

                // SPONGE FOLLOW MOUSE
                sponge.transform.position = Vector3.Lerp(sponge.transform.position, GetMouse3DPosition(), 0.2f);
            }
            break;
            case GameState.Drying:
            {
                windSourceLocked = Input.GetMouseButton(0);

                // AFFECT FUR
                if (windSourceLocked)
                {
                    if (!toolAudioSource.isPlaying)
                    {
                        toolAudioSource.Play();
                    }

                    float sum_increment = 0;
                    foreach (Fur fur in m_currentAnimal.GetFur())
                    {
                        float distance = Mathf.Sqrt(
                            Mathf.Pow(mousePosition.x- fur.transform.position.x, 2)
                            + Mathf.Pow(mousePosition.y- fur.transform.position.y, 2));
                        // float distance = Vector3.Distance(mousePosition, fur.transform.position);
                        float increment = dryingBrushStrength
                                        * Mathf.Exp(- Mathf.Pow(dryingBrushDamping * Mathf.Max(0, distance - dryingBrushRadius), 2));
                        fur.IncrementDry(increment);
                        sum_increment += increment;
                    }
                    dryer.GetComponent<Dryer>().ModifyFog(sum_increment);
                }
                else
                {
                    toolAudioSource.Stop();
                }

                // DRYER FOLLOW MOUSE
                dryer.transform.position = Vector3.Lerp(dryer.transform.position, GetMouse3DPosition(), 0.2f);
            }
            break;
            case GameState.Pimping:
            {
                if (m_waitForDryReset && !scene.isPlaying)
                {
                    m_waitForDryReset = false;
                    Prop[] props = FindObjectsOfType<Prop>();
                    foreach (Prop prop in props)
                    {
                        //prop.ResetOrigin();
                    }
                }

                if (!m_isInDryEnd && !m_waitForDryReset)
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
            case GameState.Intro:
            {
                bool isInIntro = scene.isPlaying;
                bool isInPrompt = !scene.isPlaying && textContainer.gameObject.activeSelf;
                if (!isInIntro && !isInPrompt)
                {
                    textContainer.gameObject.SetActive(true);
                    Customer customer = customers[m_currentCustomerIndex];
                    Rules rules = customer.rules[m_currentRule];
                    TMP_Text text = CustomerToText(customer);
                    text.text = rules.introSentence;
                    text.gameObject.SetActive(true);

                    Bark();
                }

                if (isInPrompt && Input.GetMouseButtonDown(0))
                {
                    m_nextState = GameState.Cleaning;
                    SetState(GameState.Transition);
                }
            }
            break;
            case GameState.Outro:
            {
                if (!scene.isPlaying)
                {
                    m_currentCustomerIndex += 1;
                    if (m_currentCustomerIndex >= customers.Length)
                    {
                        SetState(GameState.End);
                    }
                    else
                    {
                        SetState(GameState.Intro);
                    }
                }
            }
            break;
            default:
            {

            }
            break;
        }

        // TIME
        bool timeHasExpired = false;
        if (m_currentTimer >= 0.0f)
        {
            if (!tictacAudioSource.isPlaying)
            {
                tictacAudioSource.Play();
            }

            m_currentTimer -= Time.deltaTime;

            if (m_currentTimer < 0.0f)
            {
                timeHasExpired = true;
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
        else
        {
            tictacAudioSource.Stop();
        }

        // DRAWER OUT
        if (m_currentstate == GameState.Pimping && timeHasExpired)
        {
            if (m_draggedProp)
            {
                Prop prop = m_draggedProp;
                onPropDragStop();
                prop.ForceReset();

            }
            m_isInDryEnd = true;
            barkAudioSource.PlayOneShot(pingAudio);
            scene.Play("DrawerOut");
        }
        if (m_isInDryEnd && !scene.isPlaying)
        {
            SetState(GameState.Outro);
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
        timerText.text = m_currentTimer >= 0.0f ? Mathf.FloorToInt(m_currentTimer).ToString() : "";
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

    public void Bark()
    {
        Customer customer = customers[m_currentCustomerIndex];

        float bias = 0.1f;
        barkAudioSource.pitch = Random.Range(1f-bias, 1f+bias);
        barkAudioSource.PlayOneShot(customer.bark);
    }

    public void AnimalBark()
    {
        Customer customer = customers[m_currentCustomerIndex];

        float bias = 0.1f;
        barkAudioSource.pitch = Random.Range(1f-bias, 1f+bias);
        barkAudioSource.PlayOneShot(customer.animalBark);
    }

    void get_percentage_dryness(ref float wet, ref float dry, ref float burned)
    {
        burned = dry = wet = 0;
        foreach (Fur fur in m_currentAnimal.GetFur())
        {
            int indexdry = fur.GetDryIndex();
            int indexclean = fur.GetCleanIndex();
            if (indexclean == 1){
                switch (indexdry) {
                    case 0: wet += 1; break;
                    case 1: dry += 1; break;
                    case 2: burned += 1; break;
                }
            }
        }
        float sum = wet + dry + burned;
        if (sum != 0){
            wet /= sum;
            dry /= sum;
            burned /= sum;
        }
    }
    void get_percentage_cleanliness(ref float dirty, ref float clean, ref float ripped)
    {
        ripped = clean = dirty = 0;
        foreach (Fur fur in m_currentAnimal.GetFur())
        {
            int index = fur.GetCleanIndex();
            switch (index) {
                case 0: dirty += 1; break;
                case 1: clean += 1; break;
                case 2: ripped += 1; break;
            }
        }
        float sum = dirty + clean + ripped;
        dirty /= sum;
        clean /= sum;
        ripped /= sum;
    }

    Prop m_draggedProp;
    Vector3 m_draggedPropOffset;
    GameState m_currentstate;
    GameState m_nextState;

    bool m_previousWindSourceLocked = false;
    Vector3 m_windSourceOrigin;
    Vector3 m_windSourceOffset;

    float m_currentTimer = -1.0f;
    Vector3 m_previousMousePostion;

    int m_currentCustomerIndex = 0;
    int m_currentRule = 0;
    Animal m_currentAnimal;

    bool m_isInDryEnd = false;
    bool m_waitForDryReset = false;

    // TOOLS
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

    RectTransform StateToTitle(GameState _state)
    {
        RectTransform title = null;
        switch (_state)
        {
            case GameState.Cleaning: title = titleClean; break;
            case GameState.Drying: title = titleDry; break;
            case GameState.Pimping : title = titlePimp; break;
        }
        return title;
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

    void ClearAllChildren(Transform _transform)
    {
        foreach(Transform t in _transform)
        {
            Object.Destroy(t.gameObject);
        }
    }

    TMP_Text CustomerToText(Customer _customer)
    {
        TMP_Text text = null;
        switch (_customer.id)
        {
            case 0: text = justineText; break;
            case 1: text = ayaText; break;
            case 2 : text = racineText; break;
        }
        return text;
    }
}
