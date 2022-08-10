using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SOurce code: https://answers.unity.com/questions/1167177/how-do-i-get-the-current-volume-level-amplitude-of.html

public class IndicatorController : MonoBehaviour
{
    [HideInInspector]
    public AudioSource audioSource;
    //public AudioClip[] audioClips;

    [HideInInspector]
    public float updateStep = 0.1f;
    [HideInInspector]
    public int sampleDataLength = 1024;

    private float currentUpdateTime = 0f;
    [HideInInspector]
    public float clipLoudness;
    private float[] clipSampleData;

    //public AudioClip audioClip;
    public GameObject mapIndicator;
    private Transform mapIconicIndicator;
    private Transform mapTextIndicator;

    public GameObject tagIndicator;
    private Transform tagIconicIndicator;
    private Transform tagTextIndicator;
    [HideInInspector]
    public float minIndicatorScaleOnMiniMap = 0.2f;
    [HideInInspector]
    public float maxIndicatorScaleOnMiniMap = 3.0f;
    [HideInInspector]
    public float minIndicatorScaleOnEnv = 0.07f;
    [HideInInspector]
    public float maxIndicatorScaleOnEnv = 1.05f;
    [HideInInspector]
    public float scaleStep = 0.5f;

    //public bool isAutoPlay = false;

    //public bool isLoop = false;
    [HideInInspector]
    public Sprite indicatorIcon = null;
    [HideInInspector]
    public string indicatorText = "";


    void Awake()
    {
        clipSampleData = new float[sampleDataLength];
    }

    // Start is called before the first frame update
    void Start()
    {
        if (mapIndicator)
        {
            mapIconicIndicator = mapIndicator.transform.Find("Iconic Loudness Indicator");
            mapTextIndicator = mapIndicator.transform.Find("Text Loudness Indicator");

            if (indicatorIcon)
            {
                mapIconicIndicator.GetComponent<SpriteRenderer>().sprite = indicatorIcon;

                mapIconicIndicator.gameObject.SetActive(false);
            }

            if (indicatorText != "")
            {
                mapTextIndicator.GetComponent<TextMesh>().text = indicatorText;

                mapTextIndicator.gameObject.SetActive(false);
            }

            mapIndicator.SetActive(false);
        }

        if (tagIndicator)
        {
            tagIconicIndicator = tagIndicator.transform.Find("Iconic Indicator On Tag");
            tagTextIndicator = tagIndicator.transform.Find("Text Indicator On Tag");

            if (indicatorIcon)
            {
                tagIconicIndicator.GetComponent<SpriteRenderer>().sprite = indicatorIcon;

                tagIconicIndicator.gameObject.SetActive(false);
            }

            if (indicatorText != "")
            {
                tagTextIndicator.GetComponent<TextMesh>().text = indicatorText;

                tagTextIndicator.gameObject.SetActive(false);
            }

            tagIndicator.SetActive(false);
        }

        //if (isAutoPlay)
        //{
        //    StartPlaying();
        //}

        //audioSource.loop = isLoop;
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource.isPlaying)
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= updateStep)
            {
                currentUpdateTime = 0f;

                //read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                audioSource.clip.GetData(clipSampleData, audioSource.timeSamples);
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength;
            }

            if (mapIndicator)
            {
                mapIndicator.SetActive(true);

                if (SoundVizVRManager.instance.isIconicMapIndicator)
                {
                    mapIconicIndicator.gameObject.SetActive(true);
                    mapTextIndicator.gameObject.SetActive(false);
                }
                else if (SoundVizVRManager.instance.isTextMapIndicator)
                {
                    mapTextIndicator.gameObject.SetActive(true);
                    mapIconicIndicator.gameObject.SetActive(false);
                }
            }

            if (tagIndicator && SoundVizVRManager.instance.onObjectIndicatorState)
            {
                tagIndicator.SetActive(true);

                if (SoundVizVRManager.instance.isIconicTagIndicator)
                {
                    tagIconicIndicator.gameObject.SetActive(true);
                    tagTextIndicator.gameObject.SetActive(false);
                }
                else if (SoundVizVRManager.instance.isTextTagIndicator)
                {
                    tagTextIndicator.gameObject.SetActive(true);
                    tagIconicIndicator.gameObject.SetActive(false);
                }
            }

            if (tagIndicator && !SoundVizVRManager.instance.onObjectIndicatorState)
            {
                tagIconicIndicator.gameObject.SetActive(false);
                tagTextIndicator.gameObject.SetActive(false);

                tagIndicator.SetActive(false);
            }
        }
        else
        {
            if (mapIndicator)
            {
                mapIconicIndicator.gameObject.SetActive(false);
                mapTextIndicator.gameObject.SetActive(false);

                mapIndicator.SetActive(false);
            }

            if (tagIndicator)
            {
                tagIconicIndicator.gameObject.SetActive(false);
                tagTextIndicator.gameObject.SetActive(false);

                tagIndicator.SetActive(false);
            }
        }

        AnimateLoudnessIndicator();
    }

    //public void StartPlaying()
    //{
    //    if (!audioSource.isPlaying)
    //    {
    //        audioSource.clip = audioClip;
    //        audioSource.Play();
    //    }
    //}

    //public void StopPlaying()
    //{
    //    audioSource.Stop();
    //}

    void AnimateLoudnessIndicator()
    {
        float minimapTarScale = minIndicatorScaleOnMiniMap + (maxIndicatorScaleOnMiniMap - minIndicatorScaleOnMiniMap) * clipLoudness;
        float tagTarScale = minIndicatorScaleOnEnv + (maxIndicatorScaleOnEnv - minIndicatorScaleOnEnv) * clipLoudness;
        
        float minimapOffset = 1f;
        float tagOffset = 1f;

        if (SoundVizVRManager.instance.isTextMapIndicator)
        {
            minimapOffset = 0.7f;
        }

        if (SoundVizVRManager.instance.isTextTagIndicator)
        {
            tagOffset = 0.7f;
        }

        if (mapIndicator && mapIndicator.activeSelf)
        {
            mapIndicator.transform.localScale = Vector3.Lerp(new Vector3(minimapTarScale * minimapOffset, minimapTarScale * minimapOffset, minimapTarScale * minimapOffset), mapIndicator.transform.localScale, scaleStep);
        }

        if (tagIndicator && tagIndicator.activeSelf)
        {
            tagIndicator.transform.localScale = Vector3.Lerp(new Vector3(tagTarScale * tagOffset, tagTarScale * tagOffset, tagTarScale * tagOffset), tagIndicator.transform.localScale, scaleStep);
        }
    }
}
