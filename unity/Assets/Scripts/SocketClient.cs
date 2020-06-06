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

    // Use this for initialization
    void Start()
    {
        var host = "127.0.0.1";
        var port = 8000;

        // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }

        /*
        // 接受欢迎信息
        var bytes = new byte[1024];
        var count = client.Receive(bytes);
        Console.WriteLine("New message from server: {0}", Encoding.UTF32.GetString(bytes, 0, count));

        // 不断的获取输入，发送给服务端
        var input = "";
        while (input != "exit")
        {
            input = Console.ReadLine();
            client.Send(Encoding.UTF32.GetBytes(input));
        }

        client.Close();
		*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}
