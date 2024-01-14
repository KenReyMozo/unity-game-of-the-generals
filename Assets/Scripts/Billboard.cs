using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;
    Transform _t, _camT;
    void Start()
    {
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }
        if(cam == null)
        {
            enabled = false;
            return;
        }
        _camT = cam.transform;
        _t = transform;
    }

    // Update is called once per frame
    void Update()
    {
        _t.LookAt(_camT);
    }
}
