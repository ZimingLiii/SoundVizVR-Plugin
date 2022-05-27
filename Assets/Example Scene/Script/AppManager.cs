using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    [NonSerialized]
    public static AppManager instance;

    //public SoundVizVRManager.SoundVizType mySoundViz = SoundVizVRManager.SoundVizType.IconMapWithIconTag;

    [NonSerialized]
    public OVRInput.Controller activeController;


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

        // Play in 90Hz
        if (OVRManager.display != null && OVRManager.display.displayFrequenciesAvailable.Contains(90f))
        {
            OVRManager.display.displayFrequency = 90f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Access to SoundVizVRManager instance and set SoundVizType dynamically
        //SoundVizVRManager.instance.SetSoundVizState((int)mySoundViz);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
