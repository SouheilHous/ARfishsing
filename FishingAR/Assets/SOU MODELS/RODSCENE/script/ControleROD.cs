using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleROD : MonoBehaviour
{

    [Range(0, 32)] public float BulletFrames;
    private Animator rodanim;
    private float maxY=0.8F;
    private float minY=0;

    // Start is called before the first frame update
    void Start()
    {
        rodanim = this.gameObject.GetComponent<Animator>();

        
    }

    // Update is called once per frame
    void Update()
    {

        if (this.transform.position.y < maxY) {
            rodanim.SetBool("START", true);
            rodanim.Play("THROW", 0, ((minY - (this.transform.position.y)) / (minY - maxY)));
        }
   else 
        {
            if (rodanim.GetBool("START") == true) {
                rodanim.Play("IDLESTART");
                rodanim.SetBool("START", false);
            }
            

        }

    }
}
