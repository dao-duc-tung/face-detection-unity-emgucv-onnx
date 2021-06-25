using System;
using System.Collections.Generic;
using UnityEngine;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

public class CornerBB
{
    //corner-form bounding boxes(x0, y0, x1, y1)
    public float X0, Y0, X1, Y1;

    public float Width
    {
        get
        {
            return X1 - X0;
        }
        set
        {
            X1 = X0 + value;
        }
    }

    public float Height
    {
        get
        {
            return Y1 - Y0;
        }
        set
        {
            Y1 = Y0 + value;
        }
    }

    public float XCenter {
        get {
            return (float)(X0 + X1) / 2;
        }
    }

    public float YCenter {
        get {
            return (float)(Y0 + Y1) / 2;
        }
    }

    public bool IsValid()
    {
        if (Width < 0 || Height < 0) return false;
        return true;
    }
}

public class FaceBB : CornerBB
{
    public string Label { get; set; }
    public float Confidence { get; set; }
}

public class FaceDetectedEventArgs : EventArgs
{
    public IReadOnlyList<FaceBB> BoundingBoxes;
    public System.Drawing.Size OriginalSize;
}

public delegate void FaceDetectedEventHandler(object sender, FaceDetectedEventArgs eventArgs);

public interface IFaceDetector
{
    event FaceDetectedEventHandler FaceDetected;
    void LoadModel();
    bool IsModelLoaded();
    void Detect(Mat input);
}

public static class ImageUtils {
    public static Texture2D ConvertWebCamTextureToTexture2D(WebCamTexture texture) {
        Texture2D texture2D = new Texture2D(texture.width, texture.height);
        texture2D.SetPixels32(texture.GetPixels32());
        return texture2D;
    }

    public static void SetValue(this Mat mat, int row, int col, dynamic value)
    {
    }

    public static Mat ConvertWebCamTextureToMat(WebCamTexture frame, DepthType depthType, int nBytePerPixel, int channels) {
        try {
            Image<Bgra, Byte> img = new Image<Bgra, byte>(frame.width, frame.height);
            var frameData = new Color32[frame.width * frame.height];
            frame.GetPixels32(frameData);

            // Method 1: Slow - modify img data by index
            for (int r = 0; r < frame.height; ++r) {
                for (int c = 0; c < frame.width; ++c) {
                    var pixel = frameData[r * frame.width + c];
                    img[r, c] = new Bgra(pixel.b, pixel.g, pixel.r, pixel.a);
                }
            }

            // Method 2: Fast - modify img data by pointer (use byte* ptr = &img.Data[0])
            // Not implemented yet
            return img.Mat;
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
        return null;
    }
}
