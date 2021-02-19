using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacementController : MonoBehaviour
{
    [SerializeField]
    private GameObject placedPrefab;
    private bool placed;
    private int placedPrefabCount;

    public GameObject PlacedPrefab
    {
        get 
        {
            return placedPrefab;
        }
        set 
        {
            placedPrefab = value;
        }
    }

    private ARRaycastManager arRaycastManager;
    private ARPlaneManager arplaneManager;

    void Awake() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arplaneManager = GetComponent<ARPlaneManager>();
    }

    void Update()
    {
        if (!placed)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                Vector2 touchPosition = touch.position;

                if (touch.phase == TouchPhase.Began)
                {
                    bool isOverUI = touchPosition.IsPointOverUIObject();

                    if (!isOverUI && arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                    {
                        var hitPose = hits[0].pose;
                        Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
                        arplaneManager.enabled = false;
                        placedPrefabCount++;
                        placed = true;
                        UIManager.Instance.UpdateObjectCountText($"OBJECT COUNT: {placedPrefabCount}");
                    }
                }
            }
        }
       
    }

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
}
