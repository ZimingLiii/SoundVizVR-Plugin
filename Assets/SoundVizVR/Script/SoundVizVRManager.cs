using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SoundVizVRManager : MonoBehaviour
{
    [NonSerialized]
    public static SoundVizVRManager instance;

    private GameObject HUDMap;
    private Transform centerEyeAnchor;
    private GameObject playerIndicatorOnMap;

    [SerializeField]
    private GameObject OVRPlayerObject;

    [SerializeField]
    private GameObject IndicatorPrefab;

    [SerializeField]
    private GameObject HUDMapPrefab;

    [SerializeField]
    private GameObject MiniMapCameraPrefab;

    [SerializeField]
    private GameObject PlayerOnMapPrefab;

    [Serializable]
    class SoundVizIndicator
    {
        public string name = "Indicator";
        public string descriptiveText = "";
        public Sprite indicatorIcon;
        public GameObject bindingObject;
        //public AudioClip audioClip;
        //public bool isLoop = false;
        //public bool isPlayOnWake = false;
        public Vector3 indicatorScale = new Vector3(0.2f, 0.2f, 0.2f);
        public float indicatorHeight = 0f;
    }

    [SerializeField]
    private List<SoundVizIndicator> SoundVizIndicatorList;

    private List<GameObject> audioSourceList = new List<GameObject>();

    public enum SoundVizType
    {
        Off = 0,
        IconMapWithIconTag = 1,
        IconMapWithTextTag = 2,
        TextMapWithIconTag = 3,
        TextMapWithTextTag = 4
    };

    [HideInInspector]
    public bool onObjectIndicatorState = false;
    [HideInInspector]
    public bool isIconicTagIndicator = false;
    [HideInInspector]
    public bool isTextTagIndicator = false;
    [HideInInspector]
    public bool isIconicMapIndicator = false;
    [HideInInspector]
    public bool isTextMapIndicator = false;

    [NonSerialized]
    public List<string> playingSourceObjectNames = new List<string>();

    [NonSerialized]
    public List<string> playingClipNames = new List<string>();

    [SerializeField]
    private Vector2 miniMapCanvasSize = new Vector2(1110, 624);
    
    [SerializeField]
    private Vector3 miniMapPosition = new Vector3(-240, -245, 660);

    [SerializeField]
    private Vector3 miniMapTiltAngle = new Vector3(0, -20f, 0);

    [SerializeField]
    private float miniMapCameraHeight = 8.5f;

    [SerializeField]
    private float miniMapOpacity = 1f;

    // Indicator Property
    public int indicatorSampleDataLength = 1024;
    public float indicatorUpdateStep = 0.1f;
    public float minIndicatorScaleOnMiniMap = 0.2f;
    public float maxIndicatorScaleOnMiniMap = 3.0f;
    public float minIndicatorScaleOnEnv = 0.07f;
    public float maxIndicatorScaleOnEnv = 1.05f;
    public float indicatorScaleStep = 0.5f;

    public SoundVizType currentSoundVizState = SoundVizType.Off;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(transform.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PluginSetup();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LateUpdate()
    {
        UpdateSoundSourcePosition();
    }

    private void PluginSetup()
    {
        InitializePrefabObj();
        SetSoundVizState((int)currentSoundVizState);
        BindSoundSource();
    }

    private void InitializePrefabObj()
    {
        if (OVRPlayerObject == null || HUDMapPrefab == null || PlayerOnMapPrefab == null || MiniMapCameraPrefab == null)
        {
            return;
        }

        centerEyeAnchor = GameObject.Find("CenterEyeAnchor").transform;

        if (centerEyeAnchor == null)
        {
            return;
        }

        // Create Parent Canvas
        GameObject parentCanvas = new GameObject();
        parentCanvas.name = "MapCanvas";
        

        parentCanvas.AddComponent<Canvas>();

        Canvas myCanvas = parentCanvas.GetComponent<Canvas>();
        myCanvas.renderMode = RenderMode.WorldSpace;
        myCanvas.worldCamera = centerEyeAnchor.GetComponent<Camera>();
        parentCanvas.AddComponent<CanvasScaler>();
        parentCanvas.AddComponent<GraphicRaycaster>();

        parentCanvas.transform.SetParent(centerEyeAnchor);

        parentCanvas.GetComponent<RectTransform>().sizeDelta = miniMapCanvasSize;
        parentCanvas.GetComponent<RectTransform>().localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);

        GameObject miniMapAnchor = new GameObject();
        miniMapAnchor.name = "MiniMapAnchor";
        miniMapAnchor.AddComponent<RectTransform>();
        miniMapAnchor.transform.SetParent(parentCanvas.transform);
        miniMapAnchor.GetComponent<RectTransform>().localPosition = miniMapPosition;
        miniMapAnchor.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        miniMapAnchor.GetComponent<RectTransform>().localScale = Vector3.one;
        miniMapAnchor.GetComponent<RectTransform>().rotation = Quaternion.Euler(miniMapTiltAngle);

        HUDMap = Instantiate(HUDMapPrefab, Vector3.zero, Quaternion.identity);
        HUDMap.transform.SetParent(miniMapAnchor.transform);
        HUDMap.transform.localRotation = Quaternion.Euler(Vector3.zero);
        HUDMap.transform.localPosition = Vector3.zero;
        HUDMap.transform.localScale = Vector3.one;

        // MiniMap Opacity
        var myOpacity = 1f;
        if (miniMapOpacity > 1 || miniMapOpacity < 0)
        {
            myOpacity = 1f;
        }
        else
        {
            myOpacity = miniMapOpacity;
        }

        var image1 = HUDMap.gameObject.GetComponent<Image>();
        var image2 = HUDMap.transform.GetChild(0).gameObject.GetComponent<Image>();
        var image3 = HUDMap.transform.GetChild(0).GetChild(0).gameObject.GetComponent<RawImage>();

        image1.color = new Color(image1.color.r, image1.color.g, image1.color.b, myOpacity);
        image2.color = new Color(image2.color.r, image2.color.g, image2.color.b, myOpacity);
        image3.color = new Color(image3.color.r, image3.color.g, image3.color.b, myOpacity);

        playerIndicatorOnMap = Instantiate(PlayerOnMapPrefab, Vector3.zero, Quaternion.identity);
        playerIndicatorOnMap.layer = LayerMask.NameToLayer("Map");

        GameObject miniMapCamera = Instantiate(MiniMapCameraPrefab, Vector3.zero, Quaternion.identity);
        miniMapCamera.transform.position = new Vector3(0, miniMapCameraHeight, 0);
        miniMapCamera.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
    }

    private void BindSoundSource()
    {
        if (IndicatorPrefab == null)
        {
            return;
        }

        GameObject containerObj = new GameObject();
        containerObj.name = "IndicatorGroup";
        Transform container = containerObj.transform;

        for (int i = 0; i < SoundVizIndicatorList.Count; i++)
        {
            SoundVizIndicator obj = SoundVizIndicatorList[i];

            GameObject tar = obj.bindingObject;

            AudioSource tarAudioSource = tar.GetComponentsInChildren<AudioSource>()[0];

            GameObject go = Instantiate(IndicatorPrefab, tar.transform.position, Quaternion.identity);
            go.name = "Indicator_" + obj.name;
            go.transform.localScale = obj.indicatorScale;

            go.transform.Find("Tag Container").localPosition = new Vector3(0, obj.indicatorHeight, 0);

            var objIndicatorController = go.GetComponentInChildren<IndicatorController>();
            objIndicatorController.audioSource = tarAudioSource;
            objIndicatorController.indicatorIcon = obj.indicatorIcon;
            objIndicatorController.indicatorText = obj.descriptiveText;

            objIndicatorController.sampleDataLength = indicatorSampleDataLength;
            objIndicatorController.updateStep = indicatorUpdateStep;
            objIndicatorController.minIndicatorScaleOnMiniMap = minIndicatorScaleOnMiniMap;
            objIndicatorController.maxIndicatorScaleOnMiniMap = maxIndicatorScaleOnMiniMap;
            objIndicatorController.minIndicatorScaleOnEnv = minIndicatorScaleOnEnv;
            objIndicatorController.maxIndicatorScaleOnEnv = maxIndicatorScaleOnEnv;
            objIndicatorController.scaleStep = indicatorScaleStep;

            go.transform.SetParent(container);

            audioSourceList.Add(go);
        }
    }

    private void UpdateSoundSourcePosition()
    {
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            var obj = audioSourceList[i];
            var tar = SoundVizIndicatorList[i];
            if (obj.name.Equals("Indicator_" + tar.bindingObject.name))
            {
                obj.transform.position = tar.bindingObject.transform.position;
            }
        }
    }

    public void SetSoundVizState(int soundVizState)
    {
        if (HUDMap == null)
        {
            return;
        }

        InactivateMaps();

        switch ((SoundVizType)soundVizState)
        {
            case SoundVizType.IconMapWithIconTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isIconicMapIndicator = true;
                isIconicTagIndicator = true;
                currentSoundVizState = SoundVizType.IconMapWithIconTag;
                break;

            case SoundVizType.IconMapWithTextTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isIconicMapIndicator = true;
                isTextTagIndicator = true;
                currentSoundVizState = SoundVizType.IconMapWithTextTag;
                break;

            case SoundVizType.TextMapWithIconTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isTextMapIndicator = true;
                isIconicTagIndicator = true;
                currentSoundVizState = SoundVizType.TextMapWithIconTag;
                break;

            case SoundVizType.TextMapWithTextTag:
                HUDMap.SetActive(true);
                onObjectIndicatorState = true;
                isTextMapIndicator = true;
                isTextTagIndicator = true;
                currentSoundVizState = SoundVizType.TextMapWithTextTag;
                break;

            default:
                currentSoundVizState = SoundVizType.Off;
                break;
        }

        void InactivateMaps()
        {
            HUDMap.SetActive(false);

            onObjectIndicatorState = false;

            isIconicMapIndicator = false;
            isIconicTagIndicator = false;
            isTextMapIndicator = false;
            isTextTagIndicator = false;

            playerIndicatorOnMap.SetActive(true);
        }
    }
}