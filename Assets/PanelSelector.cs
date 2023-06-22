using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSelector : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject resultPanel;
    public GameObject settingsPanel;

    public SelectPanel selectPanel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(selectPanel){
            case SelectPanel.start:
                startPanel.SetActive(true);
                gamePanel.SetActive(false);
                resultPanel.SetActive(false);
                settingsPanel.SetActive(false);
                break;
            case SelectPanel.game:
                startPanel.SetActive(false);
                gamePanel.SetActive(true);
                resultPanel.SetActive(false);
                settingsPanel.SetActive(false);
                break;
            case SelectPanel.result:
                startPanel.SetActive(false);
                gamePanel.SetActive(false);
                resultPanel.SetActive(true);
                settingsPanel.SetActive(false);
                break;
            case SelectPanel.settings:
                startPanel.SetActive(false);
                gamePanel.SetActive(false);
                resultPanel.SetActive(false);
                settingsPanel.SetActive(true);
                break;
        }
    }
 
    public void SwitchStartPanel(){
        selectPanel = SelectPanel.start;
    }
    public void SwitchGamePanel(){
        selectPanel = SelectPanel.game;
    }
    public void SwitchResultPanel(){
        selectPanel = SelectPanel.result;
    }
    public void SwitchSettingsPanel(){
        selectPanel = SelectPanel.settings;
    }

    public enum SelectPanel{
        start,
        game,
        result,
        settings
    }

}
