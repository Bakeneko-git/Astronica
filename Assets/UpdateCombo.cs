using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NotesSystem;

public class UpdateCombo : MonoBehaviour
{
    public NotesManager nm;
    private TextMeshProUGUI combo_text;
    public bool maxCombo;
    // Start is called before the first frame update
    void Awake()
    {
        combo_text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(maxCombo){
            combo_text.text =  "Max Combo\n" + nm.maxcombo.ToString();
        }else{
            combo_text.text = "x" + nm.combo.ToString() + "\nCombo";
        }
    }
}
