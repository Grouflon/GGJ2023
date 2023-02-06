using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
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
    public int forceRandomRuleIndex = -1;

    [Header("Game Rules")]
    public float cleaningTime = 5.0f;
    public float dryingTime = 5.0f;
    public float pimpingTime = 5.0f;
    public Customer[] customers;
    public bool shuffleCustomers = true;

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
    public ReviewUI reviewUI;
    public Transform drawerPrefabRight;
    public Transform drawerPrefabLeft;
    public Transform drawerContainerLeft;
    public Transform drawerContainerRight;
    public AudioSource barkAudioSource;
    public AudioSource toolAudioSource;
    public AudioSource tictacAudioSource;
    public AudioClip spongeSound;
    public AudioClip dryerSound;
    public AudioClip pingAudio;
    public AudioClip takeAudio;
    public AudioClip dropAudio;
    
    // Start is called before the first frame update
    void Start()
    {
        m_scores = new int[customers.Length];

        if (shuffleCustomers)
        {
            Customer temp;
            for (int i = 0; i < customers.Length; i++)
            {
                int rnd = Random.Range(0, customers.Length);
                temp = customers[rnd];
                customers[rnd] = customers[i];
                customers[i] = temp;
            }
        }
        
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
                sponge.GetComponent<Sponge>().StopBubble();
                sponge.gameObject.SetActive(false);

                Cursor.visible = true;
                
                toolAudioSource.Stop();

                float dirty, clean, ripped;
                dirty = clean = ripped = 0;
                get_percentage_cleanliness(ref dirty, ref clean, ref ripped);

                toolAudioSource.volume = 1.0f;

            }
            break;
            case GameState.Drying:
            {
                dryer.GetComponent<Dryer>().StopFog();
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
                ClearAllChildren(drawerContainerLeft);
                ClearAllChildren(drawerContainerRight);

            }
            break;
            case GameState.Transition:
            {
                RectTransform title = StateToTitle(m_nextState);
                title.gameObject.SetActive(false);
            }
            break;
            case GameState.Intro:
            {
                textContainer.gameObject.SetActive(false);
                ayaText.gameObject.SetActive(false);
                justineText.gameObject.SetActive(false);
                racineText.gameObject.SetActive(false);
                m_promptSkipNotified = false;
            }
            break;
            case GameState.Outro:
            {

            }
            break;
            case GameState.End:
            {
                m_reviewsSkipNotified = false;
                reviewUI.gameObject.SetActive(false);
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
                toolAudioSource.volume = 0.0f;

                m_currentTimer = cleaningTime;

                sponge.gameObject.SetActive(true);
                sponge.transform.position = GetMouse3DPosition();
                sponge.GetComponent<Sponge>().StopBubble();
                Cursor.visible = false;

                toolAudioSource.clip = spongeSound;

            }
            break;
            case GameState.Drying:
            {
                toolAudioSource.volume = 0.1f;

                m_currentTimer = dryingTime;

                dryer.gameObject.SetActive(true);
                dryer.transform.position = GetMouse3DPosition();
                dryer.GetComponent<Dryer>().StopFog();
                Cursor.visible = false;
                
                toolAudioSource.clip = dryerSound;
            }
            break;
            case GameState.Pimping:
            {
                m_currentTimer = pimpingTime;
                scene.Play("DrawerIn");
                m_waitForDryReset = true;

                ClearAllChildren(drawerContainerLeft);
                ClearAllChildren(drawerContainerRight);
                Object.Instantiate(drawerPrefabLeft, drawerContainerLeft);
                Object.Instantiate(drawerPrefabRight, drawerContainerRight);
            }
            break;
            case GameState.Transition:
            {
                RectTransform title = StateToTitle(m_nextState);
                title.gameObject.SetActive(true);
                titleAnimation.Play();

                barkAudioSource.PlayOneShot(pingAudio);
            }
            break;
            case GameState.Intro:
            {
                m_promptSkipNotified = false;

                Customer customerData = customers[m_currentCustomerIndex];
                if (forceRandomRuleIndex >= 0)
                {
                    m_currentRule = forceRandomRuleIndex;
                }
                else
                {
                    m_currentRule = Random.Range(0, customerData.rules.Length); 
                }

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
                m_scores[m_currentCustomerIndex] = ComputeCurrentScore();
                scene.Play("CustomerExit");
            }
            break;
            case GameState.End:
            {
                m_reviewsSkipNotified = false;
                reviewUI.SetReviews(customers, m_scores);
                reviewUI.gameObject.SetActive(true);
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
                sponge.transform.position = Vector3.Lerp(sponge.transform.position, GetMouse3DPosition(), 0.25f);
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
                dryer.transform.position = Vector3.Lerp(dryer.transform.position, GetMouse3DPosition(), 0.25f);
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
                        m_draggedPropOffset = Vector3.Lerp(m_draggedPropOffset, Vector3.zero, 0.14f);
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

                    AnimalBark();
                }

                if (isInPrompt && m_promptSkipNotified)
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
            case GameState.End:
            {
                if (m_reviewsSkipNotified)
                {
                    SceneManager.LoadScene("SplashScreen");
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
        timerText.gameObject.SetActive(m_currentTimer >= 0.0f);
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

        barkAudioSource.PlayOneShot(takeAudio);
    }

    void onPropDragStop()
    {
        Assert.IsNotNull(m_draggedProp);

        Animal targetAnimal = GetObjectUnderMouse<Animal>();
        m_draggedProp.OnStopDrag(targetAnimal);

        m_draggedProp = null;

        barkAudioSource.PlayOneShot(dropAudio);
    }

    public void NotifyPromptButtonClicked()
    {
        m_promptSkipNotified = true;
    }

    public void NotifyReviewsButtonClicked()
    {
        m_reviewsSkipNotified = true;
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

    int ComputeCurrentScore()
    {
        Customer customer = customers[m_currentCustomerIndex];
        Rules rule = customer.rules[m_currentRule];
        
        float dirty, clean, ripped;
        dirty = clean = ripped = 0;
        get_percentage_cleanliness(ref dirty, ref clean, ref ripped);

        float wet, dry, burned;
        burned = dry = wet = 0;
        get_percentage_dryness(ref wet, ref dry, ref burned);

        bool isCleanOk = true;
        if (rule.dirtyMin != rule.dirtyMax)
        {
            isCleanOk = isCleanOk && dirty >= rule.dirtyMin && dirty <= rule.dirtyMax;
        }
        if (rule.cleanMin != rule.cleanMax)
        {
            isCleanOk = isCleanOk && clean >= rule.cleanMin && clean <= rule.cleanMax;
        }
        if (rule.rippedMin != rule.rippedMax)
        {
            isCleanOk = isCleanOk && ripped >= rule.rippedMin && ripped <= rule.rippedMax;
        }

        bool isDryOk = true;
        if (rule.wetMin != rule.wetMax)
        {
            isDryOk = isDryOk && wet >= rule.wetMin && wet <= rule.wetMax;
        }
        if (rule.dryMin != rule.dryMax)
        {
            isDryOk = isDryOk && dry >= rule.dryMin && dry <= rule.dryMax;
        }
        if (rule.burnedMin != rule.burnedMax)
        {
            isDryOk = isDryOk && burned >= rule.burnedMin && burned <= rule.burnedMax;
        }

        bool expectsPositiveProp = rule.positivePropsTags.Length > 0;
        bool hasPositiveProp = false;
        bool hasNegativeProp = false;
        Prop[] props = m_currentAnimal.GetComponentsInChildren<Prop>();
        foreach (Prop prop in props)
        {
            foreach(string propTag in prop.tags)
            {
                foreach(string ruleTag in rule.positivePropsTags)
                {
                    hasPositiveProp = hasPositiveProp || ruleTag == propTag;
                }

                foreach(string ruleTag in rule.negativePropsTags)
                {
                    hasNegativeProp = hasNegativeProp || ruleTag == propTag;
                }

                if (hasNegativeProp) break;
            }
            if (hasNegativeProp) break;
        }
        bool isPimpOk = !hasNegativeProp && (!expectsPositiveProp || hasPositiveProp);
        Debug.Log("Clean: " + isCleanOk + " / Dry: " + isDryOk + " / Pimp: " + isPimpOk);

        int score = (isCleanOk ? 1 : 0) + (isDryOk ? 1 : 0) + (isPimpOk ? 1 : 0);
        return score;
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

    int[] m_scores;

    bool m_promptSkipNotified = false;
    bool m_reviewsSkipNotified = false;

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
