using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;

public class FaceTracker : MonoBehaviour
{
    public Webcam webCam;
    private IFaceDetector faceDetector;
    private Texture2D output;
    private Color32[] data;
    private Mat origImg;
    private int markerRange = 10;
    private Color32 white = new Color32(255, 255, 255, 255);

    void Start() {
        faceDetector = new UltraFaceDetector();
        faceDetector.LoadModel();
        faceDetector.FaceDetected += _faceDetector_FaceDetected;

        output = new Texture2D(webCam.camTexture.width, webCam.camTexture.height);
        webCam.rawImage.texture = output;
        data = new Color32[webCam.camTexture.width * webCam.camTexture.height];
    }

    void Update() {
        if (webCam != null && webCam.camTexture != null) {
            ProcessFrame(webCam.camTexture);
        }
    }

    private void ProcessFrame(WebCamTexture cam) {
        try {
            origImg = ImageUtils.ConvertWebCamTextureToMat(cam, DepthType.Cv8U, 8, 4);
            faceDetector.Detect(origImg);
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    private void _faceDetector_FaceDetected(object sender, FaceDetectedEventArgs eventArgs) {
        var faceBBs = eventArgs.BoundingBoxes;
        var frame = webCam.camTexture;
        var data = frame.GetPixels32();
        foreach (var bb in faceBBs) {
            DrawMarker(frame, data, bb);
        }
        output.SetPixels32(data);
        output.Apply();
    }

    public void DrawMarker(WebCamTexture frame, Color32[] data, FaceBB faceBB) {
        var xCenter = faceBB.XCenter;
        var yCenter = faceBB.YCenter;
        for (int i = -markerRange; i < markerRange; ++i) {
            for (int j = -markerRange; j < markerRange; ++j) {
                var x = (int)xCenter + i;
                var y = (int)yCenter + j;
                data[y * frame.width + x] = white;
            }
        }
    }
}
