using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitLightCont : MonoBehaviour
{
    public Light left_light;
    public Light right_light;
    public float speed = 1;
    public bool left_sw;
    public bool right_sw;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        left_light.intensity -= Time.deltaTime * speed;
        right_light.intensity -= Time.deltaTime * speed;
        if(left_sw){
            left_light.intensity = 300;
        }
        if(right_sw){
            right_light.intensity = 300;
        }
        left_sw = false;
        right_sw = false;
    }
}
