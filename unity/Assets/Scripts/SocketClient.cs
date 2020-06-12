using System;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

enum STATE
{
    IDLE, WAIT
}

public class SocketClient : MonoBehaviour
{
    private Socket client;
    private Texture2D latestSendTexture;
    public RawImage centerProcessOutput;
    public Text recvFps;
    private List<GameObject> lineSegements;
    private List<CenterNetArray> resultQueue;
    private int[,] keypointEdges = new int[,] {{0, 1}, {0, 2}, {1, 3}, {2, 4},
                    {3, 5}, {4, 6}, {5, 6},
                    {5, 7}, {7, 9}, {6, 8}, {8, 10},
                    {5, 11}, {6, 12}, {11, 12},
                    {11, 13}, {13, 15}, {12, 14}, {14, 16}};
    private Thread socketRecv;
    private STATE socketState;

    private DateTime startTime;
    private List<int> elapsedTimesMs;

    // Use this for initialization
    void Start()
    {
        socketState = STATE.IDLE;
        startTime = DateTime.Now;
        elapsedTimesMs = new List<int>();

        resultQueue = new List<CenterNetArray>();
        lineSegements = new List<GameObject>();

        var host = "127.0.0.1";
        var port = 8000;

        // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            socketRecv = new Thread(readCenterNetResult);
            socketRecv.IsBackground = true;
            socketRecv.Start();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
    }

    private void renderCenterResult()
    {
        if (resultQueue.Count <= 0)
            return;

        CenterNetArray centerNet = resultQueue[0];

        List<Vector3> keyPoint = new List<Vector3>();
        destoryLines();
        for (int i = 0; i < centerNet.result.Length; i++)
        {
            float xScale = centerProcessOutput.rectTransform.rect.width / latestSendTexture.width;
            float yScale = centerProcessOutput.rectTransform.rect.height / latestSendTexture.height;

            //preprocess bbox coordinates
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

            //preprocess keypoints coordinates
            List<Vector3> keypoints = new List<Vector3>();
            for (int j = 0; j < centerNet.result[i].hp.Length; j += 2)
            {
                centerNet.result[i].hp[j + 1] = latestSendTexture.height - centerNet.result[i].hp[j + 1];
                centerNet.result[i].hp[j + 1] *= yScale;
                centerNet.result[i].hp[j] *= xScale;
                keypoints.Add(new Vector3(centerNet.result[i].hp[j], centerNet.result[i].hp[j + 1], 0));
            }

            // draw bounding box result
            Vector3 topLeft = new Vector3(centerNet.result[i].bbox[0], centerNet.result[i].bbox[1], 0);
            Vector3 topRight = new Vector3(centerNet.result[i].bbox[2], centerNet.result[i].bbox[1], 0);
            Vector3 bottomLeft = new Vector3(centerNet.result[i].bbox[0], centerNet.result[i].bbox[3], 0);
            Vector3 bottomRight = new Vector3(centerNet.result[i].bbox[2], centerNet.result[i].bbox[3], 0);

            List<Vector3> bbox = new List<Vector3>();
            bbox.Add(topLeft);
            bbox.Add(topRight);
            bbox.Add(topRight);
            bbox.Add(bottomRight);
            bbox.Add(bottomRight);
            bbox.Add(bottomLeft);
            bbox.Add(bottomLeft);
            bbox.Add(topLeft);
            addLines(bbox, Color.green);

            //draw keypoints edges
            for (int j = 0; j < keypointEdges.Length / 2; j++)
            {
                List<Vector3> line = new List<Vector3>();
                line.Add(keypoints[keypointEdges[j, 0]]);
                line.Add(keypoints[keypointEdges[j, 1]]);

                //左右邊身體分別為奇數/偶數,上軀幹連結(5,6),下軀幹連接(11,12)
                if ((keypointEdges[j, 0] % 2 == 0) && (keypointEdges[j, 1] % 2 == 0))
                    addLines(line, Color.red);
                else if ((keypointEdges[j, 0] % 2 == 1) && (keypointEdges[j, 1] % 2 == 1))
                    addLines(line, Color.blue);
                else
                    addLines(line, Color.magenta);


            }

        }
        recvFps.text = string.Format("FPS : {0}", 1000.0f / (float)elapsedTimesMs.Last());

        resultQueue.RemoveAt(0);
    }
    private void readCenterNetResult()
    {
        while (client.Connected)
        {
            var bytes = new byte[4096];
            var count = client.Receive(bytes);

            //UnityEngine.Debug.Log("Socket Recv : " + Encoding.UTF8.GetString(bytes, 0, count));
            CenterNetArray centerNet = JsonUtility.FromJson<CenterNetArray>(Encoding.UTF8.GetString(bytes, 0, count));
            if (centerNet.result.Length > 0)
            {
                resultQueue.Add(centerNet);
            }
            socketState = STATE.IDLE;
            //Debug.Log("recv texture CenterNet result");
            int processTime = (DateTime.Now - startTime).Milliseconds;
            elapsedTimesMs.Add(processTime);
            startTime = DateTime.Now;
        }
    }

    private void destoryLines()
    {
        for (int i = 0; i < lineSegements.Count; i++)
        {
            Destroy(lineSegements[i]);
        }
        lineSegements.Clear();
    }

    private void addLines(List<Vector3> vertices, Color c)
    {
        RawImage clonePannel = Instantiate(centerProcessOutput);
        clonePannel.GetComponent<RawImage>().color = new Color(0, 0, 0, 0);
        clonePannel.transform.SetParent(centerProcessOutput.transform.parent, false);

        LineRenderer lineRenderer = clonePannel.GetComponent<LineRenderer>();

        lineRenderer.positionCount = vertices.Count;
        lineRenderer.SetPositions(vertices.ToArray());

        lineRenderer.startColor = c;
        lineRenderer.endColor = c;

        lineSegements.Add(clonePannel.gameObject);
    }

    public void sendWebCamTexture(WebCamTexture backCam)
    {
        if (socketState == STATE.WAIT)
            return;
        socketState = STATE.WAIT;

        //Debug.Log("send texture");
        if (latestSendTexture == null)
        {
            latestSendTexture = new Texture2D(backCam.width, backCam.height);
        }
        else
        {
            Destroy(centerProcessOutput.texture);
            centerProcessOutput.texture = Instantiate(latestSendTexture);
        }
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

        client.Send(z);

    }
    // Update is called once per frame
    void Update()
    {
        renderCenterResult();
    }

    private void OnApplicationQuit()
    {
        //當應用程式結束時會自動呼叫這個函數
        socketRecv.Abort();//強制中斷當前執行緒
        Debug.LogFormat("Average fps : {0}",
          (float)elapsedTimesMs.Count * 1000 / (float)elapsedTimesMs.Sum());
    }
}
