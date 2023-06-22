using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutProcessRunner : MonoBehaviour
{
    private Process process;
    public string pyfile = @"C:\Users\Haruya\Documents\UnityProject\OutProcessTest\printout.py";
    // Start is called before the first frame update
    void Start()
    {
        process = new Process();
        process.StartInfo = new ProcessStartInfo()
        {
            FileName = "cmd",
            Arguments = "/c py " + pyfile, //引数
            UseShellExecute = false,
            CreateNoWindow = false,

            RedirectStandardOutput = true, // ログ出力に必要な設定(1)
            RedirectStandardError = true,
            RedirectStandardInput = true//書き込む設定
        };

        process.EnableRaisingEvents = true;
        process.Exited += new EventHandler(proc_Exited);
        
        process.OutputDataReceived += this.OnStdOut; // ログの出力先の指定(2)
        process.ErrorDataReceived += this.OnStdError;

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 標準出力のほう
    public void OnStdOut(object sender, DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("[StdOut] " + e.Data);
    }

    // エラー出力のほう
    public void OnStdError(object sender, DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("[StdError] " + e.Data);
    }

    public void proc_Exited(object sender, EventArgs e)
    {
        UnityEngine.Debug.Log("python終了");
    }
}
