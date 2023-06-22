using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class UDPConecter : MonoBehaviour
{
    //静的メンバ
    //接続されたかどうか
    public static bool conected = false;
    //データアップデートがされたか
    public static bool updated;
    //取得した各点のポジションデータ
    public static Vector2[] posdata;
    //UDPクライアント
    private static UdpClient udp;

    //受信ポート設定
    public int LOCAL_PORT = 50007;
    
    [Header("読み取り専用")]
    //遅延
    public float dt;
    
    //スレッド
    private Thread thread;
    //遅延ログ
    private static FixedQueue<float> delayTimes = new FixedQueue<float>(10);
    //前回のUpdate時のゲーム時間
    public static float oldUpdateTime;

    //初期化
    void Awake ()
    {
        conected = false;
        udp = new UdpClient(LOCAL_PORT);
        udp.Client.ReceiveTimeout = 1000;
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start(); 
    }

    void LateUpdate ()
    {
        if(updated){
            delayTimes.Enqueue(DeltaTime());
            dt = DelayTime();
            
            oldUpdateTime = Time.time;
        }
        updated = false;
    }

    void OnApplicationQuit()
    {
        thread.Abort();
        udp.Close();
    }

    //遅延ログから平均遅延時間を取得する関数
    public float DelayTime(){
        float sums = 0;
        foreach(float x in delayTimes){
            sums += x;
        }
        return sums / delayTimes.Count;
    }
    
    //前回更新時の時間と現在時刻との差
    public static float DeltaTime(){
        return Time.time - oldUpdateTime;
    }

    //別スレ動作（UDP通信処理）
    private static void ThreadMethod()
    {
        while(true)
        {
            try{
                IPEndPoint remoteEP = null;
                byte[] data = udp.Receive(ref remoteEP);
                string text = Encoding.ASCII.GetString(data);
                //Debug.Log(text);

                string[] datas = Encoding.ASCII.GetString(data).Split(",");
                int posdata_length = (int)(datas.Length/2);
                posdata = new Vector2[posdata_length];
                for(int i = 0; i < posdata_length; i++){
                    posdata[i] = new Vector2(-float.Parse(datas[i * 2]), float.Parse(datas[i * 2 + 1]));
                }
                conected = true;
                updated = true;
            }
            catch (Exception e)
            {
                conected = false;
                // 例外が発生したときの処理
                //Debug.Log(e.ToString());
                Debug.Log("未接続");
            }
        }
    }


    #if UNITY_EDITOR
    //UDP切断処理
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            thread.Abort();
            udp.Close();
        }
    }
    #endif
}
