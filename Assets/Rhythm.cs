using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotesSystem;

public class Rhythm : MonoBehaviour
{
    public float bpm = 120f; // BPM
    public float scaleTime = 1f; // Scale変更アニメーションの再生時間

    public AudioSource audioSource;
    public AudioClip bass;
    public AudioClip sym;

    private Animation anim;
    private float lastBeatTime;
    private float nextBeatTime;
    private float beatInterval;
    private bool flag = false;
    private int beatcount;

    
    public NotesManager nm;

    void Start()
    {
        anim = GetComponent<Animation>();
        InitTimeCalc();
    }

    void InitTimeCalc(){
        beatcount = 0;
        lastBeatTime = Time.time;
        nextBeatTime = lastBeatTime + (60f / bpm);
        beatInterval = nextBeatTime - lastBeatTime;
    }

    void Update()
    {
        if(flag){
            if (Time.time >= nextBeatTime)
            {
                //beatcount
                beatcount %= 4;

                // 次のビート時間を計算する
                lastBeatTime = nextBeatTime;
                nextBeatTime += beatInterval;

                // AnimationState.speedを設定してアニメーションを再生する
                anim.clip = anim.GetClip(beatcount == 0? "Rhythm" : "RhythmSub");
                AnimationState animState = (beatcount == 0? anim["Rhythm"] : anim["RhythmSub"]);
                animState.speed = beatInterval / scaleTime;
                anim.Play();
                if(audioSource != null && bass != null && beatcount != 0)    audioSource.PlayOneShot(bass);
                if(audioSource != null && sym != null && beatcount == 0)    audioSource.PlayOneShot(sym);

                beatcount++;
            }
        }
        
    }
    public void RhythmFlag(bool f){
        flag = f;
        if(!f){
            InitTimeCalc();
        }
    }

    public void RhythomAdjusterFlag(bool f){
        flag = f;
        if(!f){
            InitTimeCalc();
        }else{
            StartCoroutine(GenerateNotes());
        }
    }

    private IEnumerator GenerateNotes()
    {
        while(true){
            if(!flag) yield break;
            yield return new WaitForSecondsRealtime(nm.far / nm.speed);
            nm.NotesGenerate(NOTESTYPE.slash,JUDGETYPE.left_hand,Vector2.zero,180);
        }
    }
}
