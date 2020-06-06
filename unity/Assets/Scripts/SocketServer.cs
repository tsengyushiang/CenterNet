using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketServer : MonoBehaviour
{
    private int port = 8000;
    private string host = "127.0.0.1";
    private Socket listener;

    // Use this for initialization
    void Start()
    {
        // 构建Socket实例、设置端口号和监听队列大小
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Parse(host), port));
        listener.Listen(5);
        Debug.Log("Waiting for connect...");
    }

    // Update is called once per frame
    void Update()
    {
        var clientExecutor = listener.Accept();
        Task.Factory.StartNew(() =>
        {
            // 获取客户端信息，C#对(ip+端口号)进行了封装。
            var remote = clientExecutor.RemoteEndPoint;
            Debug.Log(remote);

            // 发送一个欢迎消息
            clientExecutor.Send(Encoding.UTF32.GetBytes("Welcome"));

            // 进入死循环，读取客户端发送的信息
            var bytes = new byte[1024];
            while (true)
            {
                var count = clientExecutor.Receive(bytes);
                var msg = Encoding.UTF32.GetString(bytes, 0, count);
                if (msg == "exit")
                {
                    Debug.Log(remote);
                    break;
                }
                Debug.Log(msg);
                Array.Clear(bytes, 0, count);
            }
            clientExecutor.Close();
            Debug.Log(remote);
        });
    }
}
