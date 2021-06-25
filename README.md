<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/dao-duc-tung/face-detection-unity-emgucv-onnx">
    <img src="media/banner.png" alt="Logo" width="300" height="100">
  </a>

  <h3 align="center">Face Detection on Unity using Emgu CV and ONNX</h3>

  <p align="center">
    <a href="https://github.com/dao-duc-tung/face-detection-unity-emgucv-onnx/issues">Report Bug</a>
    ·
    <a href="https://github.com/dao-duc-tung/face-detection-unity-emgucv-onnx/issues">Request Feature</a>
  </p>
</p>


<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li><a href="#getting-started">Getting Started</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>


<!-- ABOUT THE PROJECT -->
## About The Project

![product-screenshot][product-screenshot]

The project integrates an ONNX-based Face Detection CNN Model into a Windows-based Unity application by using Emgu CV Dnn module for inference.

The purpose of the project is to mainly show how to integrate Emgu CV (latest version) into an Unity application for **FREE** in **3 steps** without buying [Emgu CV v4.x in Unity Asset Store](https://assetstore.unity.com/packages/tools/ai/emgu-cv-v4-x-144853) and how to interact with image data from a PC webcam.

The inference implementation of the ONNX model using Emgu CV was actually done in my previous project [Face Detection on UWP using ONNX](https://github.com/dao-duc-tung/face-detection-uwp-onnx).

Project is built with
- Emgu.CV v4.5.1.4349
- Emgu.CV.Bitmap v4.5.1.4349
- Emgu.CV.runtime.windows v4.5.1.4349
- Unity v2020.3.12f1


<!-- GETTING STARTED -->
## Getting Started

1. Clone the repository and open project folder in Unity (Just click `Ignore` when it shows error)

2. Install NuGet Package Manager
  - Download and install NuGet for Unity [here](https://github.com/GlitchEnzo/NuGetForUnity/releases)
  - Close and reopen your Unity project

3. Install Emgu CV in Unity
  - In the menu of Unity, select NuGet > Manage NuGet Packages
  - Search for `emgu`, install `Emgu.CV`, `Emgu.CV.runtime.windows`, and `Emgu.CV.Bitmap` by Emgu Corporation

4. Install `cvextern.dll`
  - Download Emgu CV [here](https://github.com/emgucv/emgucv/releases). Make sure the version is matched with Emgu CV in the previous step.
  - After installing, go to folder `libs\x64`, i.e. `C:\Emgu\emgucv-windesktop 4.5.1.4349\libs\x64`, copy file `cvextern.dll` to the `Assets\Plugins` folder in your Unity project

5. Open `Assets\Scenes\SampleScene` and play app in Unity


<!-- CONTRIBUTING -->
## Contributing

Contributions make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/FeatureName`)
3. Commit your Changes (`git commit -m 'Add some FeatureName'`)
4. Push to the Branch (`git push origin feature/FeatureName`)
5. Open a Pull Request

<!-- LICENSE -->
## License

Distributed under the MIT License. See [LICENSE](LICENSE) for more information.

<!-- CONTACT -->
## Contact

Tung Dao - [LinkedIn](https://www.linkedin.com/in/tungdao17/)

Project Link: [https://github.com/dao-duc-tung/face-detection-unity-emgucv-onnx](https://github.com/dao-duc-tung/face-detection-unity-emgucv-onnx)

<!-- ACKNOWLEDGEMENTS -->
## Acknowledgements

- [Ultra-lightweight face detection model](https://github.com/Linzaer/Ultra-Light-Fast-Generic-Face-Detector-1MB)

<!-- MARKDOWN LINKS & IMAGES -->
[product-screenshot]: media/demo1.gif
