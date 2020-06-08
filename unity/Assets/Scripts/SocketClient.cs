using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class CenterNetArray
{
    public CenterNet[] result;
}

[Serializable]
class CenterNet
{
    public float[] bbox;
    public float[] hp;
}

public class SocketClient : MonoBehaviour
{
    private Socket client;
    private Texture2D latestSendTexture;
    public LineRenderer lineRenderer;
    public RawImage centerProcessOutput;

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
        latestSendTexture = new Texture2D(100, 100);
        centerProcessOutput.texture = latestSendTexture;
    }

    void Draw(Texture2D MyTexture, float x1, float y1, float x2, float y2, Color c)
    {
        float x, y;
        float dy = y2 - y1;
        float dx = x2 - x1;
        float m = dy / dx;
        float dy_inc = -1;

        if (dy < 0)
            dy = 1;

        float dx_inc = 1;
        if (dx < 0)
            dx = -1;

        if (Mathf.Abs(dy) > Mathf.Abs(dx))
        {
            for (y = y2; y < y1; y += dy_inc)
            {
                x = x1 + (y - y1) * m;
                MyTexture.SetPixel((int)(x), (int)(y), c);
            }
        }
        else
        {
            for (x = x1; x < x2; x += dx_inc)
            {
                y = y1 + (x - x1) * m;
                MyTexture.SetPixel((int)(x), (int)(y), c);
            }
        }
        MyTexture.Apply();
    }

    public void sendWebCamTexture(WebCamTexture backCam)
    {
        latestSendTexture = new Texture2D(backCam.width, backCam.height);
        latestSendTexture.SetPixels(backCam.GetPixels());
        latestSendTexture.Apply();

        byte[] frame = ImageConversion.EncodeToJPG(latestSendTexture);

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
            var bytes = new byte[4096];
            var count = client.Receive(bytes);

            //UnityEngine.Debug.Log(Encoding.UTF8.GetString(bytes, 0, count));

            CenterNetArray centerNet = JsonUtility.FromJson<CenterNetArray>(Encoding.UTF8.GetString(bytes, 0, count));

            if (centerNet.result.Length > 0)
            {
                List<Vector3> bbox = new List<Vector3>();
                List<Vector3> keyPoint = new List<Vector3>();
                for (int i = 0; i < centerNet.result.Length; i++)
                {
                    float xScale = centerProcessOutput.rectTransform.rect.width / latestSendTexture.width;
                    float yScale = centerProcessOutput.rectTransform.rect.height / latestSendTexture.height;

                    //preprocess coordinates
                    for (int j = 0; j < centerNet.result[i].bbox.Length; j++)
                    {
                        if (j % 2 == 1)
                        {
                            centerNet.result[i].bbox[j] = latestSendTexture.height - centerNet.result[i].bbox[j];
                            centerNet.result[i].bbox[j] *= yScale;
                        }
                        else
                        {
                            centerNet.result[i].bbox[j] *= xScale;

                        }
                    }

                    // draw bounding box result
                    Vector3 topLeft = new Vector3(centerNet.result[i].bbox[0], centerNet.result[i].bbox[1], 0);
                    Vector3 topRight = new Vector3(centerNet.result[i].bbox[2], centerNet.result[i].bbox[1], 0);
                    Vector3 bottomLeft = new Vector3(centerNet.result[i].bbox[0], centerNet.result[i].bbox[3], 0);
                    Vector3 bottomRight = new Vector3(centerNet.result[i].bbox[2], centerNet.result[i].bbox[3], 0);

                    bbox.Add(topLeft);
                    bbox.Add(topRight);
                    bbox.Add(topRight);
                    bbox.Add(bottomRight);
                    bbox.Add(bottomRight);
                    bbox.Add(bottomLeft);
                    bbox.Add(bottomLeft);
                    bbox.Add(topLeft);

                    //draw 

                }

                lineRenderer.positionCount = bbox.Count;
                lineRenderer.SetPositions(bbox.ToArray());

                centerProcessOutput.texture = latestSendTexture;
            }
        }
    }
}
