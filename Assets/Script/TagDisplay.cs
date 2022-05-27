using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagDisplay : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        FaceCamera();
    }

    // Update is called once per frame
    void Update()
    {
        FaceCamera();
    }

    private void FaceCamera()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward);
    }
}
