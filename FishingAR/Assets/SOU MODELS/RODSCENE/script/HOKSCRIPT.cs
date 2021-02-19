using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOKSCRIPT : MonoBehaviour
{
    public GameObject rodPos;
    public GameObject HOCKJOINPARENT;
    public Animator fish;
    public Animator RodAnimator;
    public float HokPositionY;
    // boolean to set true if fishing is correct and goes well 
    public bool Fished;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //set float Floorlevel of animator to control animation when rod is close to floor play throw and when get out of water play catch 
        HokPositionY = rodPos.transform.position.y - 0.2F;
        if (fish != null)
        {
            fish.SetFloat("FLOORLEVEL", HokPositionY);
        }
        RodAnimator.SetFloat("FLOORLEVEL", HokPositionY); 
    }
    private void OnTriggerEnter(Collider other)
    {
        //catch and play fish animation to turn around 
        if (other.tag == "fish")
        {
            fish = other.gameObject.GetComponent<Animator>();
            //here start fishing and if Fished boolean is false then he fail and cant set position to parent 
            fish.SetTrigger("TRYCATSH");
            if (Fished == true)
            {
                other.transform.position = HOCKJOINPARENT.transform.position;
                other.transform.SetParent(HOCKJOINPARENT.transform) ;
                
                fish.SetBool("CATCHED", true);
            }
        }
    }
}
