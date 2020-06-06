using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class StartPython : MonoBehaviour
{

    private string AnacondaActivateBat = "C:/Users/m10815098/anaconda3/Scripts/activate.bat";
    private string AnacondaEnv = "CenterNet";
    private string workingDirectory = "C:/Users/m10815098/Desktop/CenterNet/python/src";
    private string pythonExeCommand = "python demo.py multi_pose --demo webcam --load_model ../models/multi_pose_dla_3x.pth";
    private Process process;
    public void Start()
    {
        startPythonProcess();
    }

    void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log(e.Data);
    }

    //主要分為ProcessStartInfo設定檔的設定與Process的啟動兩個部分。
    public void startPythonProcess()
    {
        // Set working directory and create process
        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            }
        };
        process.Start();
        // Pass multiple commands to cmd.exe
        using (var sw = process.StandardInput)
        {
            if (sw.BaseStream.CanWrite)
            {
                // Vital to activate Anaconda
                sw.WriteLine(AnacondaActivateBat);
                // Activate your environment
                sw.WriteLine("activate " + AnacondaEnv);
                // run your script. You can also pass in arguments
                sw.WriteLine(pythonExeCommand);
            }
        }

        // read multiple output lines
        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine();
            UnityEngine.Debug.Log(line);
        }
    }

    public void OnDestroy()
    {
        stopPythonProcess();
    }
    public void stopPythonProcess()
    {
        process.Kill();
    }
}
