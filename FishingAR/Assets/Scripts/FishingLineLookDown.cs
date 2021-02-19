using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
using UniRx.Triggers;

public class FishingLineLookDown : MonoBehaviour
{
    [SerializeField] Transform lookedAt;
    [SerializeField] Camera cam;

    private void Start()
    {
        this.UpdateAsObservable()
               .Subscribe(_ => LookDown())
               .AddTo(this);
    }


    void LookDown()
    {
        lookedAt.position = cam.transform.position + Vector3.up * 10;
        transform.LookAt(lookedAt);
    }
}
