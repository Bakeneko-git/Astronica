/*
・ノーツの移動
・ノーツごとの判定
・アニメーション制御
・

*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotesSystem;

public class Notes : MonoBehaviour
{
    public int notes_id;
    public NotesManager nm;
    private Animator animator;
    public float speed = 1;

    public NOTESTYPE notes_type;
    public JUDGETYPE judge_type;

    private Vector2 old_dir = Vector2.zero;
    public Vector2 lastDirection;
    public bool hit = false;

    public bool adjustMode = false;

    public AudioSource audioSource;
    public AudioClip audioClip;
    public int judge_count;


    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this object.");
        }
    }

    void Start(){

    }

    void Update()
    {
        //Z方向に移動
        transform.Translate(0,0,-Time.deltaTime * speed);
    }

    private bool isHit(){
        //判定処理
        if(nm.isHit(this)){
            animator.SetBool("hit",true);
            nm.Hit(this);
            return true;
        }
        return false;
    }

    IEnumerator WaitingUpdatedForHit()
    {
        while (UDPConecter.updated) // 次のアップデートまでまつ
        {
            //遅延が一定以上になったら処理しない。
            if(UDPConecter.DeltaTime() >= 0.2f)  yield break;
            yield return null;
        }
        isHit();
    }

    public void HitAnimation(){
        animator.SetBool("hit",true);
    }
}

