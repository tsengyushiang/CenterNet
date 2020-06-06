using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct Color32Array
{
    public byte[] byteArray;
    public Color32[] colors;
}

public class ScriptsCamera : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private WebCamTexture frontCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;
    private Socket client;
    // Use this for initialization
    private void Start()
    {
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
                backCam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);

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


        var host = "127.0.0.1";
        var port = 8000;

        // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }

    }

    // Update is called once per frame
    private void Update()
    {
        if (!camAvailable)
            return;


        Texture2D t = new Texture2D(backCam.width, backCam.height);
        t.SetPixels(backCam.GetPixels());
        t.Apply();

        byte[] frame = ImageConversion.EncodeToJPG(t);

        int length = frame.Length;
        byte[] payload = BitConverter.GetBytes(length);

        byte tmp = payload[0];
        payload[0] = payload[3];
        payload[3] = tmp;
        tmp = payload[1];
        payload[1] = payload[2];
        payload[2] = tmp;

        var z = new byte[payload.Length + frame.Length];
        payload.CopyTo(z, 0);
        frame.CopyTo(z, payload.Length);

        UnityEngine.Debug.Log(z[0].ToString() + " " +
         z[1].ToString() + " " +
         z[2].ToString() + " " +
         z[3].ToString() + "," + length);

        
        client.Send(z);
        

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        //background.rectTransform.localScale = new Vector3 (1f, scaleY, 1f);    //非鏡像
        background.rectTransform.localScale = new Vector3(-1f, scaleY, 1f);    //鏡像

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }
}