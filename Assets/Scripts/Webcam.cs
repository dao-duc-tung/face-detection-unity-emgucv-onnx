using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    public RawImage rawImage;
    public AspectRatioFitter fitter;

    [NonSerialized]
    public WebCamTexture camTexture;
    private bool camAvailable = false;
    private Texture defaultBackground;
    private bool isFrontCam = false;

    void Start()
    {
        defaultBackground = rawImage.texture;
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0) {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }
        camTexture = new WebCamTexture(devices[0].name, Screen.width, Screen.height);
        if (camTexture == null) {
            Debug.Log("Unable to open camera");
            return;
        }
        isFrontCam = devices[0].isFrontFacing;
        camTexture.Play();
        camAvailable = true;
    }

    void Update()
    {
        if (!camAvailable) return;
        float ratio = (float)camTexture.width / camTexture.height;
        fitter.aspectRatio = ratio;

        float scaleX = isFrontCam ? -1f : 1f;
        float scaleY = camTexture.videoVerticallyMirrored ? -1f: 1f;
        rawImage.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        int orient = -camTexture.videoRotationAngle;
        rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }
}
