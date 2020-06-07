using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    private Socket client;

    // Use this for initialization
    void Start()
    {
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

    public void sendWebCamTexture(WebCamTexture backCam)
    {
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

        /*
        UnityEngine.Debug.Log(z[0].ToString() + " " +
         z[1].ToString() + " " +
         z[2].ToString() + " " +
         z[3].ToString() + "," + length);
		*/

        client.Send(z);

    }
    // Update is called once per frame
    void Update()
    {
        if (client.Connected)
        {
            var bytes = new byte[1024];
            var count = client.Receive(bytes);
            UnityEngine.Debug.Log(Encoding.UTF8.GetString(bytes, 0, count));
        }
    }
}
