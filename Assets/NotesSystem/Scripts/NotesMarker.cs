using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using NotesSystem;


namespace NotesMarkers{
    public class NotesMarker: Marker, INotification
    {
        
        //JUDGETYPE
        [HideInInspector]
        public SLASHJUDGE sj;
        //hand
        [HideInInspector]
        public LR lr;
        //向き
        [HideInInspector]
        public DIR dir;

        //メニューここから
        public NOTESTYPE type;
        public JUDGETYPE judge;
        public Vector2 pos;
        public float rot;
        
        public PropertyName id
        {
            get
            {
                return new PropertyName("method");
            }
        }
    }

    public enum POS{
        left = -1,
        right = 1,
        center = 0
    }
    
    public enum DIR{
        up = 0,
        down = 180,
        left = -90,
        right = 90,
        up_left = -45,
        up_right = 45,
        down_left = -135,
        down_right = 135,
    }

    public enum LR{
        left = -1,
        right = 1
    }

    public enum SLASHJUDGE{
        left = 0,
        right = 1,
    }
}