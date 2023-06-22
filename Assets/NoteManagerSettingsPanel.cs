using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NotesSystem;

public class NoteManagerSettingsPanel : MonoBehaviour
{
    public NotesManager notesManager;
    public Slider slashMoveSlider;
    public Slider slashRotSlider;
    public Slider circleSlider;
    // Start is called before the first frame update
    void Awake(){
        slashMoveSlider.value = notesManager.move_threshold;
        slashRotSlider.value = notesManager.rot_threshold;
        circleSlider.value = notesManager.circle_threshold;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangedSlashMoveSlider(){
        notesManager.move_threshold = slashMoveSlider.value;
    }
    public void OnChangedSlashRotSlider(){
        notesManager.rot_threshold = slashRotSlider.value;
    }
    public void OnChangedCircleSlider(){
        notesManager.circle_threshold = circleSlider.value;
    }
}
