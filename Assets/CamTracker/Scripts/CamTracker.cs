/*

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CamTrackingSystem{
    public class CamTracker : MonoBehaviour
    {
        public TRACKER tracker;
        public Vector2 position_offset = new Vector2(0,0);
        public bool link_tracker = true;
        public TRACKER reference_tracker;
        public Vector2 scale_offset = new Vector2(2,2);

        public Vector2 save_position;
        
        public FixedQueue<Vector2> pos_log = new FixedQueue<Vector2>(4);

        public Vector2 movement;
        private Vector3 _posmovement = Vector3.zero;
        public float speed;
        private Vector2 old_pos;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(UDPConecter.updated){
                //link_trackerが有効ならば、offsetに基準のTracker座標を入れる
                if(link_tracker)  position_offset = GetPosdata(reference_tracker);
                //現在のポジションをセーブ
                save_position = GetPosdata(tracker);
                //offsetした値をセット
                transform.localPosition = (Vector3)offset_position(save_position);

                //pos_logに保存
                pos_log.Enqueue(save_position);
                //前回からの移動値
                movement = save_position - old_pos;
                _posmovement = (Vector3)movement;
                //speed
                speed = movement.magnitude;
                //前回の位置として保存
                old_pos = save_position;
            }else{
                //これまでの記録から次の点を予測し仮に移動しておく。
                //offsetした値をセット
                _posmovement /= 2; 
                transform.Translate((Vector3)(_posmovement)/2);
            }
        }

        
        //
        private Vector2 offset_position(Vector2 v){
            return (v - position_offset) * scale_offset;
        }

        //演算メソッド
        //二点の中心
        private static Vector2 center_vec2(Vector2 v1, Vector2 v2){
            return (v1 + v2) / 2;
        }

        //外部からTrackerの位置情報にアクセスするためのメソッド
        public static Vector2 GetPosdata(TRACKER t){
            Vector2 result = Vector2.zero;
            if(UDPConecter.conected){
                if((int)t < 50){
                    result = UDPConecter.posdata[(int)t];
                }else{
                    switch(t){
                        case TRACKER.center_hip:
                            result = center_vec2(UDPConecter.posdata[(int)TRACKER.left_hip], UDPConecter.posdata[(int)TRACKER.right_hip]);
                            break;
                        case TRACKER.center_shoulder:
                            result = center_vec2(UDPConecter.posdata[(int)TRACKER.left_shoulder], UDPConecter.posdata[(int)TRACKER.right_shoulder]);
                            break;
                        default:
                            break;
                    }
                }
            }
            return result;
        }
    }
    
    public enum TRACKER
    {
        nose = 0,
        left_hand = 1,
        right_hand = 2,

        left_hip = 3,
        right_hip = 4,
        
        center_hip = 50,
        
        left_shoulder = 5,
        right_shoulder = 6,

        center_shoulder = 51
    }
}


public class FixedQueue<T> : IEnumerable<T>
{
    private Queue<T> _queue;

    public int Count => _queue.Count;

    public int Capacity { get; private set; }

    public FixedQueue(int capacity)
    {
        Capacity = capacity;
        _queue = new Queue<T>(capacity);
    }

    public T Enqueue(T item)
    {
        _queue.Enqueue(item);

        if (Count > Capacity) return Dequeue();
        else return Peek();
    }

    public T Dequeue() => _queue.Dequeue();

    public T Peek() => _queue.Peek();
    public virtual void Clear () => _queue.Clear();

    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();

    public T[] ToArray(){
        T[] result = new T[_queue.Count];
        int i = 0;
        foreach(var item in _queue){
            result[i] = item;
            i++;
        }
        return result;
    }
}
