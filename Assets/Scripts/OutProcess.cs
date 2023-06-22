using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutProcess : MonoBehaviour
{
    
    private string command = @"c:\windows\system32\ipconfig.exe";
    
    // Start is called before the first frame update
    void Start()
    {

        ProcessStartInfo psInfo = new ProcessStartInfo();

        psInfo.FileName = command; // 実行するファイル
        psInfo.CreateNoWindow = true; // コンソール・ウィンドウを開かない
        psInfo.UseShellExecute = false; // シェル機能を使用しない

        psInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト

        Process p = Process.Start(psInfo); // アプリの実行開始
        string output = p.StandardOutput.ReadToEnd(); // 標準出力の読み取り

        output = output.Replace("\r\r\n", "\n"); // 改行コードの修正
        UnityEngine.Debug.Log(output); // ［出力］ウィンドウに出力
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
