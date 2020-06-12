using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptsCamera : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private WebCamTexture frontCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;
    public SocketClient CSharpSocketClient;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
    }
    // Use this for initialization
    private void Start()
    {
        Application.targetFrameRate = 500;

        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }
        for (int i = 0; i < devices.Length; i++)
        {

            //if (!devices [i].isFrontFacing) {    //開啟後鏡頭
            if (devices[i].isFrontFacing)
            {    //開啟前鏡頭
                backCam = new WebCamTexture(devices[i].name, 128,128);

            }
        }
        if (backCam == null)
        {
            Debug.Log("Unable to find back camera");
            return;
        }
        backCam.Play();
        background.texture = backCam;

        camAvailable = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!camAvailable)
            return;

        CSharpSocketClient.sendWebCamTexture(backCam);
    }
}