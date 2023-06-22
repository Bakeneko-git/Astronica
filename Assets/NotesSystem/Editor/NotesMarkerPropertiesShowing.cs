#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CamTrackingSystem;
using NotesSystem;
using NotesMarkers;

[CustomEditor(typeof(NotesMarker))]
[CanEditMultipleObjects]
public class NotesMarkerPropertiesShowing : Editor
{
    private NotesMarker _target;
    
    //メニューここから
    private NOTESTYPE type = NOTESTYPE.slash;
    private JUDGETYPE judge = JUDGETYPE.left_hand;
    private Vector2 pos = Vector2.zero;
    private float rot = 0;

    //変更箇所検知
    private bool isNotesType = false;
    private bool isSlashJudge = false;
    private bool isPos = false;
    private bool isRot = false;


    private void OnEnable()
    {
        _target = target as NotesMarker;
    }

    public override void OnInspectorGUI() {
        
        serializedObject.Update();
        

        var type_prop = serializedObject.FindProperty("type");
        var judge_prop = serializedObject.FindProperty("judge");
        var pos_prop = serializedObject.FindProperty("pos");
        var rot_prop = serializedObject.FindProperty("rot");

        //NotesMarker fnm =  serializedObject.targetObjects[0] as NotesMarker;
        NOTESTYPE type = _target.type;
        JUDGETYPE judge = _target.judge;
        Vector2 pos = _target.pos;
        float rot = _target.rot;

        
        //JUDGETYPE
        SLASHJUDGE sj = _target.sj;
        //hand
        LR lr = _target.lr;
        //向き
        DIR dir = _target.dir;

        bool is_hdv_NotesType = false;
        
        foreach (Object obj in serializedObject.targetObjects)
        {
            NotesMarker obj_marker = obj as NotesMarker;
            if (obj_marker != null){
                if(obj_marker.type != type)  is_hdv_NotesType = true;
            }
        }

        //複数選択で異なるノーツタイプが選択されている
        if (is_hdv_NotesType)  EditorGUILayout.HelpBox("複数の異なるNOTES TYPEが選択されています。", MessageType.Info);
        
        // 列挙型の選択を表示するGUIを作成
        EditorGUI.BeginChangeCheck();
        type = (NOTESTYPE)EditorGUILayout.EnumPopup("Type", type);
        if(EditorGUI.EndChangeCheck()) isNotesType = true;
        
        // 選択された列挙型に応じて表示するGUIを変更    
        switch (type) {
            case NOTESTYPE.slash:
                //表示更新
                EditorGUI.BeginChangeCheck();
                sj = (SLASHJUDGE)EditorGUILayout.EnumPopup("Hand", sj);
                if(EditorGUI.EndChangeCheck()) isSlashJudge = true;

                EditorGUI.BeginChangeCheck();
                lr = (LR)EditorGUILayout.EnumPopup("Pos", lr);
                if(EditorGUI.EndChangeCheck()) isPos = true;

                EditorGUI.BeginChangeCheck();
                dir = (DIR)EditorGUILayout.EnumPopup("Dir", dir);
                if(EditorGUI.EndChangeCheck()) isRot = true;

                break;
            case NOTESTYPE.circle:
                //表示更新
                EditorGUI.BeginChangeCheck();
                sj = (SLASHJUDGE)EditorGUILayout.EnumPopup("Hand", sj);
                if(EditorGUI.EndChangeCheck()) isSlashJudge = true;

                EditorGUI.BeginChangeCheck();
                lr = (LR)EditorGUILayout.EnumPopup("Pos", lr);
                if(EditorGUI.EndChangeCheck()) isPos = true;

                break;
            case NOTESTYPE.lane:
                EditorGUI.BeginChangeCheck();
                lr = (LR)EditorGUILayout.EnumPopup("Pos", lr);
                if(EditorGUI.EndChangeCheck()) isPos = true;
                break;
        }
        
        // 編集中のオブジェクトをループ
        foreach (var obj in serializedObject.targetObjects)
        {
            NotesMarker mk = (NotesMarker)obj;
            //if(isNotesType){
                mk.type = type;
            //}
            SetData(mk.type,sj,lr,dir,out JUDGETYPE j, out  Vector2 p, out float r);
            judge = j;
            pos = p;
            rot = r;
            // 列挙型の選択を表示するGUIを作成
            //一括更新
            if(isSlashJudge && mk.type == NOTESTYPE.slash){
                mk.sj = sj;
                mk.judge = judge;

            }
            if(isPos){
                mk.lr = lr;
                mk.pos = pos;
            }
            if(isRot){
                mk.dir = dir;
                mk.rot = rot;
            }
        }

        // GUIの更新があったら実行
        if (GUI.changed)
        {
            
            EditorUtility.SetDirty(_target);
            
            isNotesType = false;
            isSlashJudge = false;
            isPos = false;
            isRot = false;
        }
    }

    private void SetData(NOTESTYPE nt,SLASHJUDGE sj,LR lr,DIR dir, out JUDGETYPE j, out Vector2 p, out float r){
        // 選択された列挙型に応じて表示するGUIを変更    
        switch (nt) {
            case NOTESTYPE.slash:
                //最終
                j = (JUDGETYPE)(int)sj;
                p = new Vector2(1.5f * (int)lr, 0);
                r = -(float)dir;

                break;
            case NOTESTYPE.circle:

                //最終
                j = (JUDGETYPE)(int)sj;
                p = new Vector2(1.5f * (int)lr, 0);
                r = 0;
                break;
            case NOTESTYPE.lane:
                //最終
                j = JUDGETYPE.center_shoulder;
                p = new Vector2(1.25f * (int)lr, 0);
                r = 0;
                Debug.Log(r);
                break;
            default:
                //最終
                j = JUDGETYPE.center_shoulder;
                p = Vector2.zero;
                r = -(float)dir;
                break;
        }
    }
    
}
#endif