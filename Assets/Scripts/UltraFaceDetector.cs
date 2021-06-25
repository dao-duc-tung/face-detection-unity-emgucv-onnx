using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using UnityEngine;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using Emgu.CV.Util;

public class UltraFaceDetector : IFaceDetector {
    private bool isLoaded = false;
    private Net ultraface;
    private Mat originalImage;
    private string modelPath =  Application.dataPath + @"\Plugins\version-RFB-320_without_postprocessing.onnx";
    private const float CONFIDENCE_THRESHOLD = 0.7f;
    private const float IOU_THRESHOLD = 0.0f;
    private const int LIMIT_MAX_FACES = 200;
    private string[] outNames = new string[]
    {
        "scores", "boxes"
    };
    private List<float> STRIDES = new List<float>
    {
        8.0f, 16.0f, 32.0f, 64.0f
    };
    private List<List<float>> featuremapSize = new List<List<float>>();
    private List<List<float>> shrinkageSize = new List<List<float>>();
    private const int NUM_FEATUREMAP = 4;
    private List<List<float>> MIN_BOXES = new List<List<float>>()
    {
        new List<float> {10.0f,  16.0f,  24.0f},
        new List<float> {32.0f,  48.0f},
        new List<float> {64.0f,  96.0f},
        new List<float> {128.0f, 192.0f, 256.0f}
    };
    private List<List<float>> priors = new List<List<float>>();
    private int numAnchors = 0;
    private const float CENTER_VARIANCE = 0.1f;
    private const float SIZE_VARIANCE = 0.2f;

    private struct InputImageSettings
    {
        public const int NumberOfChannels = 3;
        public const int ImageHeight = 240;
        public const int ImageWidth = 320;
    }

    public event FaceDetectedEventHandler FaceDetected;
    public bool IsModelLoaded() => isLoaded;

    public void LoadModel() {
        try
        {
            ultraface = DnnInvoke.ReadNetFromONNX(modelPath);
            BuildPriors();
            isLoaded = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void BuildPriors()
    {
        List<float> widthHeightList = new List<float>()
        {
            InputImageSettings.ImageWidth, InputImageSettings.ImageHeight
        };
        foreach (var size in widthHeightList)
        {
            List<float> fmItem = new List<float>();
            foreach (float stride in STRIDES)
            {
                fmItem.Add(Mathf.Ceil(size / stride));
            }
            featuremapSize.Add(fmItem);
        }

        foreach (var _ in widthHeightList)
        {
            shrinkageSize.Add(STRIDES);
        }

        // Generate prior anchors
        for (int index = 0; index < NUM_FEATUREMAP; ++index)
        {
            float scaleWidth = InputImageSettings.ImageWidth / shrinkageSize[0][index];
            float scaleHeight = InputImageSettings.ImageHeight / shrinkageSize[1][index];
            for (int j = 0; j < featuremapSize[1][index]; ++j)
            {
                for (int i = 0; i < featuremapSize[0][index]; ++i)
                {
                    float xCenter = (i + 0.5f) / scaleWidth;
                    float yCenter = (j + 0.5f) / scaleHeight;
                    foreach (float k in MIN_BOXES[index])
                    {
                        float w = k / InputImageSettings.ImageWidth;
                        float h = k / InputImageSettings.ImageHeight;
                        priors.Add(new List<float>()
                            {
                                Clip(xCenter, 1), Clip(yCenter, 1), Clip(w, 1), Clip(h, 1)
                            }
                        );
                    }
                }
            }
        }
        numAnchors = priors.Count;
        // generate prior anchors finished
    }

    public void Detect(Mat orig) {
        if (!isLoaded) throw new Exception("Model is not loaded");
        if (orig == null) throw new NullReferenceException("Input Mat is null");
        originalImage = orig;
        var input = Preprocess(orig);
        var output = _Detect(input);
        var faceBBs = Postprocess(output);
        RaiseFaceDetectedEvent(faceBBs, orig.Size);
    }

    private Mat Preprocess(Mat img) {
        int nChannel = InputImageSettings.NumberOfChannels;
        Mat rgbImage = new Mat(new Size(originalImage.Width, originalImage.Height), originalImage.Depth, nChannel);
        var conversion = originalImage.NumberOfChannels == 4 ? ColorConversion.Bgra2Rgb : ColorConversion.Bgr2Rgb;
        CvInvoke.CvtColor(originalImage, rgbImage, conversion);

        Mat inputBlob = DnnInvoke.BlobFromImage(
            rgbImage, 1.0 / 128,
            new Size(InputImageSettings.ImageWidth, InputImageSettings.ImageHeight),
            new MCvScalar(127, 127, 127), true
        );
        return inputBlob;
    }

    private VectorOfMat _Detect(Mat inputBlob) {
        VectorOfMat outBlobs = new VectorOfMat(2);
        ultraface.SetInput(inputBlob);
        ultraface.Forward(outBlobs, outNames);
        return outBlobs;
    }

    private IReadOnlyList<FaceBB> Postprocess(VectorOfMat outBlobs) {
        Mat confidencesMat = outBlobs[0];
        Mat boxesMat = outBlobs[1];
        int confidencesLen = 1 * numAnchors * 2;
        int boxesLen = 1 * numAnchors * 4;
        float[] confidences = new float[confidencesLen];
        float[] boxes = new float[boxesLen];
        Marshal.Copy(confidencesMat.DataPointer, confidences, 0, confidencesLen);
        Marshal.Copy(boxesMat.DataPointer, boxes, 0, boxesLen);
        var boxCandidates = FilterConfidences(confidences, boxes);
        var predictions = HardNMS(boxCandidates);
        var picked = predictions.GetRange(0, Math.Min(predictions.Count, LIMIT_MAX_FACES));
        return picked;
    }

    private List<FaceBB> FilterConfidences(IReadOnlyList<float> confidences, IReadOnlyList<float> boxes)
    {
        var origWidth = originalImage.Width;
        var origHeight = originalImage.Height;
        List<FaceBB> boxCandidates = new List<FaceBB>();
        for (int i = 0; i < numAnchors; ++i)
        {
            float score = confidences[2 * i + 1];
            if (score > CONFIDENCE_THRESHOLD)
            {
                int boxIdx = i * 4;
                float xCenter = boxes[boxIdx] * CENTER_VARIANCE * priors[i][2] + priors[i][0];
                float yCenter = boxes[boxIdx + 1] * CENTER_VARIANCE * priors[i][3] + priors[i][1];
                float w = Mathf.Exp(boxes[boxIdx + 2] * SIZE_VARIANCE) * priors[i][2];
                float h = Mathf.Exp(boxes[boxIdx + 3] * SIZE_VARIANCE) * priors[i][3];
                FaceBB bb = new FaceBB() {
                    X0 = Clip(xCenter - w / 2.0f, 1) * origWidth,
                    Y0 = Clip(yCenter - h / 2.0f, 1) * origHeight,
                    X1 = Clip(xCenter + w / 2.0f, 1) * origWidth,
                    Y1 = Clip(yCenter + h / 2.0f, 1) * origHeight,
                    Confidence = Clip(score, 1),
                };
                boxCandidates.Add(bb);
            }
        }
        return boxCandidates;
    }

    private List<FaceBB> HardNMS(List<FaceBB> boxCandidates)
    {
        // Do Non-Maximum Suppression to remove overlapping boxes
        boxCandidates.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
        List<FaceBB> predictions = new List<FaceBB>();
        while (boxCandidates.Count != 0)
        {
            FaceBB topbox = boxCandidates[0];
            // Remove top box
            boxCandidates.RemoveAt(0);
            predictions.Add(topbox);
            // Copy remaining boxes to a new list
            List<FaceBB> remaining_boxes = new List<FaceBB>(boxCandidates);
            foreach (FaceBB box in remaining_boxes)
            {
                // Check IOU between top box and each remaining box
                if (GetIoU(predictions[predictions.Count - 1], box) > IOU_THRESHOLD)
                {
                    boxCandidates.Remove(box);
                }
            }
        }
        return predictions;
    }

    private static float GetIoU(FaceBB bb1, FaceBB bb2)
    {
        // Calculate Intersection over Union ratio of 2 boxes.
        float x_left = Math.Max(bb1.X0, bb2.X0);
        float y_top = Math.Max(bb1.Y0, bb2.Y0);
        float x_right = Math.Min(bb1.X1, bb2.X1);
        float y_bottom = Math.Min(bb1.Y1, bb2.Y1);

        if ((x_right < x_left) || (y_bottom < y_top))
        {
            return 0.0F;
        }

        float intersection_area = (x_right - x_left) * (y_bottom - y_top);

        float bb1_area = (bb1.X1 - bb1.X0) * (bb1.Y1 - bb1.Y0);
        float bb2_area = (bb2.X1 - bb2.X0) * (bb2.Y1 - bb2.Y0);

        float iou = intersection_area / (bb1_area + bb2_area - intersection_area + 1e-5f);

        if (iou < 0 || iou > 1)
        {
            Debug.Log(iou);
            throw new ArgumentOutOfRangeException("iou not [0,1]");
        }
        return iou;
    }

    private static float Clip(float x, float y)
    {
        return (x < 0 ? 0 : (x > y ? y : x));
    }

    private void RaiseFaceDetectedEvent(IReadOnlyList<FaceBB> faces, Size originalSize)
    {
        var eventArgs = new FaceDetectedEventArgs() { BoundingBoxes = faces, OriginalSize = originalSize };
        FaceDetected?.Invoke(this, eventArgs);
    }
}
