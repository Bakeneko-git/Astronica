/*
・Timelineからノーツ生成通知を受け取る
・ノーツを生成する
・HITしたかどうか（API）
・ミスしたかどうか
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CamTrackingSystem;
using NotesMarkers;

namespace NotesSystem{
    public class NotesManager : MonoBehaviour, INotificationReceiver
    {
        //判定に用いるトラッカー郡
        [Header("判定に用いるトラッカー")]
        public HitCamTrackers hit_cam_trackers;
        //ノーツのprefab郡
        [Header("ノーツのPrefab")]
        public NotesPrefabs notes_prefabs;
        
        [Header("ノーツ設定")]
        //出現位置
        public float far = 10;
        //流れるスピード
        public float speed = 1;
        //(private)出現してから何秒後に判定が行われるべきか
        private double timing_length;

        [Header("光らせるバックライト")]
        //ライト設定
        public HitLightCont hlc;

        [Header("ノーツのヒット判定に関わる設定項目")]
        public float move_threshold = 0.003f;
        public float rot_threshold = 45f;
        public float circle_threshold = 0.05f;
        public float judge_offset = 0.05f;

        public int judge_count = 5;

        [Header("デバッグ用機能に切り替える")]
        public bool adjustMode = false;

        [Header("音の設定")]
        private AudioSource audioSource;
        public AudioClip se;

        [Header("読み取り専用")]
        public int combo = 0;
        public int maxcombo = 0;

        //ノーツ生成回数
        private int notes_generate_count = 0;
        
        private PlayableDirector playableDirector;
        private TimelineAsset timelineAsset;
        private AudioPlayableAsset audioPlayableAsset;
        private double musicStart;


        void Awake()
        {
            //Timeline編集の都合上、Timelineから音楽を流すのではなく、AudioSouceで再生する。
            //AudioSouceを取得
            audioSource = GetComponent<AudioSource>();
            playableDirector = GetComponent<PlayableDirector>();

            //デバッグログに音楽をどの程度ずらすか出力
            // (ヒットまで何秒かかるか) = far / speed
            timing_length = far / speed;

            // TimelineAssetからAudioPlayableAssetを取得する
            timelineAsset = playableDirector.playableAsset as TimelineAsset;
            //音楽トラックを取得
            AudioTrack audioTrack = timelineAsset.GetOutputTracks().ElementAt(1) as AudioTrack;
            //ミュート
            audioTrack.muted = true;

            //TimelineClip取得
            TimelineClip audioClipTimelineClip = audioTrack.GetClips().ElementAt(0);
            //スタートタイミングを計算
            musicStart = (audioClipTimelineClip.start + (double)timing_length);
            //AudioSouceに音楽設定
            audioSource.clip = (audioClipTimelineClip.asset as AudioPlayableAsset).clip;
        }

        void Start(){
            //TimelineStart();
        }

        // Update is called once per frame
        void Update()
        {

        }

        //再生
        public void TimelineStart(){
            //combo初期化
            combo = 0;
            maxcombo = 0;

            //音楽の再生を停止
            audioSource.Stop();

            StartCoroutine(PlayAudioDelayed((float)musicStart));
            // PlayableDirectorを再生する
            playableDirector.Play();
            // ノーツに合わせた再生時間に再生
            //audioSource.PlayScheduled(AudioSettings.dspTime + musicStart);
            Debug.Log(musicStart);
        }

        
        IEnumerator PlayAudioDelayed(float delayTime)
        {
            yield return new WaitForSeconds(delayTime); // 指定した時間を待つ
            audioSource.Play(); // AudioSourceを再生
        }


        //ノーツ生成
        //・ノーツ（prefab）
        //・判定方法（手/体/両手）
        //・位置（Vector2）
        //・向き(float)
        public void NotesGenerate(NOTESTYPE notes_type, JUDGETYPE judge_type, Vector2 position, float rot){
            GameObject np = null;

            switch(notes_type){
                case NOTESTYPE.slash:
                    if      (judge_type == JUDGETYPE.left_hand)     np = notes_prefabs.left_slash_notes;
                    else if (judge_type == JUDGETYPE.right_hand)     np = notes_prefabs.right_slash_notes;
                    else    return;
                    break;
                case NOTESTYPE.circle:
                    if      (judge_type == JUDGETYPE.left_hand)     np = notes_prefabs.left_circle_notes;
                    else if (judge_type == JUDGETYPE.right_hand)     np = notes_prefabs.right_circle_notes;
                    else    return;
                    break;
                case NOTESTYPE.lane:
                    if      (position.x < 0)    np = notes_prefabs.left_lane_notes;
                    else                        np = notes_prefabs.right_lane_notes;
                    break;
                default:
                    return;
            }
            GameObject notes;

            //Notes生成
            notes = Instantiate(
                np,
                transform.position + new Vector3(position.x,position.y,far),
                Quaternion.AngleAxis(rot, Vector3.forward),
                transform
            );

            //生成時にNotesの設定をする
            Notes n = notes.GetComponent<Notes>();
            //ノーツの種類を設定(判定時読み込み用)
            n.notes_type = notes_type;
            //judgetype
            n.judge_type = judge_type;
            //NotesManager設定
            n.nm = this;
            //流れる速度
            n.speed = speed;
            //通過音
            n.audioSource = audioSource;
            n.audioClip = se;

            //Adjustモード
            n.adjustMode = adjustMode;

            //判定残回数
            n.judge_count = judge_count;

            //判定処理コルーチンをスタート
            StartCoroutine(JudgeNotesHit(n));
            //デストロイも予約
            Destroy(notes, (far + 10)/speed);

            //Generate回数
            n.notes_id = notes_generate_count;
            notes_generate_count++;
        }


        //Timelineからノーツ生成通知を受け取る
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var element = notification as NotesMarker;
            if (!element)
                return;

            NotesGenerate(element.type,element.judge, element.pos, element.rot);
        }

        //ヒットしたかどうかの処理
        //hit判定に必要な情報
        /*
        ・ノーツの情報
            ・NotesManagerにある各種判定用Tracker
            ・ノーツの判定方法（向きスラッシュ/ステップ/ぐるぐる/ダブル）
            ・判定レーン（左/右）
            ・判定範囲内でのトラッカーの動きログ
        */
        public bool isHit(Notes notes){
            bool result = false;

            float th2 = move_threshold * move_threshold;

            //使用するCamTrackerの配列
            CamTracker[] cts;
            switch(notes.judge_type){
                case JUDGETYPE.left_hand:
                    cts = new CamTracker[]{hit_cam_trackers.left_hand};
                    break;
                case JUDGETYPE.right_hand:
                    cts = new CamTracker[]{hit_cam_trackers.right_hand};
                    break;
                case JUDGETYPE.center_shoulder:
                    cts = new CamTracker[]{hit_cam_trackers.center_shoulder};
                    break;
                case JUDGETYPE.center_hip:
                    cts = new CamTracker[]{hit_cam_trackers.center_hip};
                    break;
                case JUDGETYPE.double_hand:
                    cts = new CamTracker[]{hit_cam_trackers.left_hand,hit_cam_trackers.right_hand};
                    break;
                default:
                    return false;   //該当するものがなければ、ヒットなし判定を強制
            }

            bool isMove = true;
            //一定以上動きがあるか
            foreach(CamTracker ct in cts){
                //動いていないものがあれば抜ける
                if(ct.movement.sqrMagnitude < th2){
                    isMove = false;
                    break;
                }
            }

            switch(notes.notes_type){
                case NOTESTYPE.slash:
                    //動いている　かつ　該当するTrackerの動きがオブジェクトの方向と概ね一致
                    //Debug.Log(notes.notes_type.ToString()+":"+ isMove +":"+Quaternion.Angle(Quaternion.LookRotation(Vector3.forward,(Vector3)cts[0].movement),notes.transform.localRotation));
                    result = (isMove && Quaternion.Angle(Quaternion.LookRotation(Vector3.forward,(Vector3)cts[0].movement),notes.transform.localRotation) <= rot_threshold);
                    break;
                case NOTESTYPE.circle:
                    //円運動かどうかを判定する。
                    result = IsCircle(cts[0].pos_log.ToArray(),circle_threshold);
                    break;
                case NOTESTYPE.lane:
                    //体がノーツのポジション方向である
                    float notes_x = notes.transform.localPosition.x;
                    float body_x = cts[0].save_position.x;
                    result = ((notes_x > 0 && body_x > 0) || (notes_x < 0 && body_x < 0));
                    break;
                default:
                    break;
            }

            return result;
        }

        
        //Notesからの呼び出し
        //ヒットした時
        public void Hit(Notes notes){
            //hitアニメーション再生
            notes.HitAnimation();
            combo++;
            if(maxcombo < combo)    maxcombo = combo;
            Debug.Log("hit");
            if(notes.transform.localPosition.x < 0){
                hlc.left_sw = true;
            }else{
                hlc.right_sw = true;
            }
        }
        
        //ミスしたときの処理
        public void Miss(){
            Debug.Log("miss");
            combo = 0;
        }

        IEnumerator JudgeNotesHit(Notes notes){
            while(true){
                //判定範囲に入っていないならパス
                if(notes.transform.localPosition.z > judge_offset)  yield return null;
                if(notes.judge_count <= 0){
                    Miss();
                    yield break;
                }

                //判定範囲にきた。
                if(notes.transform.localPosition.z <= judge_offset) {
                    //判定処理
                    notes.hit = isHit(notes);
                    //hitしたら
                    if(notes.hit){
                        //hit処理
                        Hit(notes);
                        //コルーチン終了
                        yield break;
                    }
                    
                    notes.judge_count--;
                }
                yield return null;
            }
        }


        //円かどうかの判定
        private static bool IsCircle(Vector2[] positions, float threshold = 0.2f)
        {
            if (positions == null || positions.Length < 3)
                return false;

            float distanceSquaredSum = 0f;
            Vector2 center = Vector2.zero;

            // 中心点を求める
            for (int i = 0; i < positions.Length; i++)
            {
                center += positions[i];
            }
            center /= positions.Length;

            // 各点と中心点の距離の2乗を求め、合計値を計算する
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 diff = positions[i] - center;
                distanceSquaredSum += diff.sqrMagnitude;
            }

            // 各点と中心点の距離の平均値を計算する
            float meanDistanceSquared = distanceSquaredSum / positions.Length;

            // 各点と中心点の距離が平均値に一定の割合以内であれば円とみなす
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 diff = positions[i] - center;
                float distanceSquared = diff.sqrMagnitude;
                float ratio = distanceSquared / meanDistanceSquared;
                if (Mathf.Abs(1f - ratio) > threshold)
                    return false;
            }

            return true;
        }

        //トラッカー郡構造体
        [System.Serializable]
        public struct HitCamTrackers
        {
            public CamTracker left_hand;
            
            public CamTracker right_hand;

            public CamTracker center_shoulder;

            public CamTracker center_hip;
        }

        //ノーツPrefab構造体
        [System.Serializable]
        public struct NotesPrefabs{
            public GameObject left_slash_notes;
            public GameObject right_slash_notes;
            public GameObject left_circle_notes;
            public GameObject right_circle_notes;
            public GameObject left_lane_notes;
            public GameObject right_lane_notes;
        }
    }
    
    //ノーツタイプ
    public enum NOTESTYPE{
        slash = 0,
        circle = 1,
        lane = 2,
    }

    //判定タイプ
    public enum JUDGETYPE{
        left_hand = 0,
        right_hand = 1,
        double_hand = 2,
        center_shoulder = 3,
        center_hip = 4,
    }

    public class GameScore
    {
        public int hit = 0;
        public int miss = 0;
        public int combo = 0;
    }
}