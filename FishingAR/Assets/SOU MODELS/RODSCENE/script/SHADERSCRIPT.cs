using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHADERSCRIPT : MonoBehaviour
{
    public GameObject mesh;
    [SerializeField] Material[] Normal;
    [SerializeField] Material[] black;
    private float Yvalueofset = 0.1f;
    [SerializeField] Material ShadowBlack;
    // this script has the role of change  shader depend to Y value of fish with adding ofset 
    void Start()
    {

        Normal = mesh.gameObject.GetComponent<Renderer>().materials;
        for (int i = 0; i < Normal.Length; i++)
        {
            Debug.Log(i);
            black[i] = ShadowBlack;
        }
    }
    void Update()
    {
        if (this.transform.position.y > Yvalueofset)
        {
            mesh.gameObject.GetComponent<Renderer>().materials = Normal;
        }
        else
        {
            mesh.gameObject.GetComponent<Renderer>().materials = black;
        }
    }
   
}
