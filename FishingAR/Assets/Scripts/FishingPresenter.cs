using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine.UI;
public class FishingPresenter : MonoBehaviour
{
    [SerializeField] GameObject hookObject;
    [SerializeField] GameObject hookObjectBackSide;
    [SerializeField] Camera MainCam;
    [SerializeField] GameObject RodParent;
    public PlayGroundManager playGroundManager;
    private List<GameObject> lastCatchedFish=new List<GameObject>();
    [SerializeField] Slider sliderRot;
    private bool mouseClicked;
    private bool reachCatchPoint;
    private bool fishCatched;
    public Text[] fishScore;
    [SerializeField] GameObject[] particleEffectUI;
    [SerializeField] Transform particuleParent;
    [SerializeField] Transform targetPos;
    public float Timer;
    public bool gameStart;
    private Vector3 netStartRotation;
    [SerializeField] Text timerText;
    private bool dragging = false;
    public ReactiveProperty<bool> wrongSwing = new ReactiveProperty<bool>();
    public int netDestroyTimer=4;
    bool partState;
    int layerMask = 1 << 5;
    private float distance;
    Vector3 initialPosition;
    [SerializeField] Sprite[] catchedFishImage;
    [SerializeField] Text catchedFishMessage;
    [SerializeField] Image catchedFishImagePanel;
    [SerializeField] GameObject sawFishPanelWarn;
    [SerializeField] GameObject catchedFishPanel;
    [SerializeField] SoundView soundManager;
    public ReactiveProperty<bool> destroyNet = new ReactiveProperty<bool>();
    // Start is called before the first frame update
    void Start()
    {
        layerMask = ~layerMask;

        destroyNet.Value = false;
        partEffectOff();
        netStartRotation = RodParent.transform.localEulerAngles;
        CatchFish();
        ObserveMouseDragRod();
    }
    void CatchFish()
    {
        hookObject.OnTriggerEnterAsObservable()
            .Where(_=>playerDataModel.canCatch==true)
            .Where(_ => gameStart == true)
            .Where(_ => _.tag == "fish")
            .Where(_ => _.transform.parent.GetComponent<PathController>().IsJumping == true)
            .Do(_ => particuleParent.gameObject.SetActive(true))
            .Do(_ => particuleParent.position = hookObject.transform.position)
            .Do(_ => partEffectOn())
            .Do(_ => _.transform.parent.GetComponent<PathController>().catched = true)
            .Do(_ => _.transform.parent.position = hookObject.transform.position)
            .Do(_ => fishCatched = true)
            //.Do(_ => MainCam.GetComponent<CameraShake>().shakecamera())
            .Do(_ => _.GetComponent<Animator>().enabled = false)
            .Do(_ => _.transform.localPosition = Vector3.zero)
            .Do(_ => setFishCatched(_.transform.parent))
            .Delay(TimeSpan.FromMilliseconds(2000))
            .Do(_ => catchedFishPanel.SetActive(false))
            .Do(_ => catchedFishPanel.GetComponent<Animator>().enabled = false)
            .Do(_ => sawFishPanelWarn.SetActive(false))
            .Do(_=>particuleParent.gameObject.SetActive(false))
            .Where(_=>lastCatchedFish!=null)
            .Do(_=> clearFish())
            .Delay(TimeSpan.FromMilliseconds(1000))
            .Do(_=> playerDataModel.canJump = true)
            .Do(_=>soundManager.playCurrentActionSFX(soundManager.effectCatch))
            .Do(_ => partEffectOff())
            .Subscribe()
            .AddTo(this);
        hookObjectBackSide.OnTriggerEnterAsObservable()
            .Where(_ => gameStart == true)
            .Where(_ => _.tag == "fish")
            .Do(_ => wrongSwing.Value = true)
            .Delay(TimeSpan.FromSeconds(2))
            .Do(_ => wrongSwing.Value = false)
            .Do(_=>playerDataModel.canCatch = true)
            .Subscribe()
            .AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => fishCatched == true)
            .Do(_=>gotoPos())
            .Do(_ => reachCatchPoint = false)  
            .Where(_=> lastCatchedFish[lastCatchedFish.Count - 1]!=null)
            .Subscribe(_ => moveTocatchPost(lastCatchedFish[lastCatchedFish.Count - 1].transform, hookObject.transform.GetChild(0).transform))
            .AddTo(this);
     
        destroyNet
            .Where(_ => destroyNet.Value == true)
            .Do(_ => RodParent.transform.GetChild(0).gameObject.SetActive(false))
            .Delay(TimeSpan.FromSeconds(netDestroyTimer))
            .Do(_ => RodParent.transform.GetChild(0).gameObject.SetActive(true))
            .Do(_ => destroyNet.Value = false)
            .Subscribe()
            .AddTo(this);
        wrongSwing
            .Where(_ => wrongSwing.Value == true)
            .Subscribe(_ => playerDataModel.canCatch = false)
            .AddTo(this);

    }
    void setFishCatched(Transform fish)
    {

        fish.SetParent(hookObject.transform);
        fish.GetComponent<PathController>().enabled = false;
        lastCatchedFish.Add (fish.gameObject);
        catchedFishShow(fish.name);
    }
    void clearFish()
    {
       
        for (int i = 0; i < lastCatchedFish.Count; i++)
        {
            if (lastCatchedFish[i] != null)
            {
                checkFish(lastCatchedFish[i]);
                Destroy(lastCatchedFish[i]);
                if (FindObjectOfType<PlayGroundManager>() != null)
                {
                    playGroundManager = FindObjectOfType<PlayGroundManager>();
                }
                if (playGroundManager != null)
                {
                    
                    playGroundManager.fishInSceneReactive.Value -= 1;

                }
            }
        }
        fishCatched = false;

    }
    void moveTocatchPost(Transform fish,Transform targetpos)
    {
        if (fish.position != targetpos.position)
        {
            fish.position = Vector3.MoveTowards(fish.position, targetpos.position, 0.08f * Time.deltaTime);
        }
        else
        {
            reachCatchPoint = true;
            
        }
    }
    // Update is called once per frame
    void Update()
    {
      if (playerDataModel.currentGameStatus.Value != playerDataModel.GameStatus.OnTimeUp)
        {
            if (gameStart == true)
            {
                playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGame;
                Timer -= Time.deltaTime;
                if (Timer > 60)
                {
                    timerText.text = "Time Left : " + (int)(Timer/60)+ " min";

                }
                else
                timerText.text = "Time Left : "+ (int)Timer;
                if (Timer <= 0)
                {
                    playerDataModel.lastGameScore = 0;
                    timerText.text = "Time Left : 00" ;
                    
                        playerDataModel.lastGameScore += int.Parse(fishScore[0].text);
                    
                    gameStart = false;
                    playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimeUp;
                }
            }
          
        }
    }
    void ObserveMouseDragRod()
    {
        
      
        this.UpdateAsObservable()
            
           .Do(_ => observeMouseFun())
           .Subscribe()
           .AddTo(this);


       
            
    }
    void observeMouseFun()
    {
        if (playerDataModel.currentGameStatus.Value == playerDataModel.GameStatus.OnGame)
        {
            if (Input.GetMouseButton(0))
            {
                mouseClicked = true;
                moveRod();
            }
            if (Input.GetMouseButtonDown(0))
            {
                setRay();
            }
            if (Input.GetMouseButtonUp(0))
            {
                mouseClicked = false;
                dragging = false;
            }
        }
        

    }
       
    void moveRod()
    {

        Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        RodParent.transform.position = rayPoint;
        RodParent.transform.localPosition = new Vector3(RodParent.transform.localPosition.x*10, RodParent.transform.localPosition.y*10, 0.55f);


    }
    void setRay()
    {
        Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitpoint;
        if(Physics.Raycast(ray ,out hitpoint, layerMask))
        {
            
            if (hitpoint.transform.tag != "ui")
            {
                initialPosition = transform.position;

                Vector3 rayPoint = ray.GetPoint(0);

                distance = Vector3.Distance(transform.position, rayPoint);
            }
           
        }
        
    }
    void gotoPos()
    {
        if (targetPos != null)
        {
            moveParticule(targetPos.position);
        }
        
    }
    public void changeRotation()
    {
        RodParent.transform.localEulerAngles = new Vector3(RodParent.transform.localEulerAngles.x, netStartRotation.y+sliderRot.value, RodParent.transform.localEulerAngles.z);
    }
    
    void partEffectOn()
    {
        soundManager.playCurrentActionSFX(soundManager.effectCatch);
        if (partState == true)
        {
            partState = false;
            for (int i = 0; i == particleEffectUI.Length; i++)
            {

                particleEffectUI[i].SetActive(true);


            }
        }
        

    }
    void partEffectOff()
    {
       
        for (int i = 0; i == particleEffectUI.Length; i++)
        {
           
                particleEffectUI[i].SetActive(false);
            

        }
        partState = true;
     

    }

    void moveParticule(Vector3 uiTargetPos)
    {
        if(particuleParent.position!= uiTargetPos)
        particuleParent.position = Vector3.MoveTowards(particuleParent.position, uiTargetPos, 1f * Time.deltaTime);
       
        

    }
    void checkFish(GameObject fish)
    {
        int score = new int();
        switch (fish.name)
        {
            default:
            case "blue(Clone)":
                targetPos = fishScore[0].gameObject.transform;
                score = int.Parse(fishScore[0].text) + 1;
                fishScore[0].text = score.ToString();
                break;
            case "Red(Clone)":
                targetPos = fishScore[0].gameObject.transform;
                score = int.Parse(fishScore[0].text) + 1;
                fishScore[1].text = score.ToString();
                break;
            case "green(Clone)":
                targetPos = fishScore[0].gameObject.transform;
                score = int.Parse(fishScore[2].text) + 1;
                fishScore[2].text = score.ToString();
                break;
            case "rimbowFish(Clone)":
                
                    if (int.Parse(fishScore[0].text) != 0)
                    {
                        score = int.Parse(fishScore[0].text) * 2;

                    }
                    else
                    {
                        score = int.Parse(fishScore[0].text) + 2;

                    }
                    fishScore[0].text = score.ToString();
                
                targetPos = fishScore[0].gameObject.transform;
                break;
            case "SawFish(Clone)":
                for (int i = 0; i < fishScore.Length; i++)
                {
                    score = 0;
                    fishScore[i].text = score.ToString();
                }
                targetPos = fishScore[UnityEngine.Random.Range(0, 2)].gameObject.transform;
                break;
        }
    }
    public void resetScore()
    {
        int score = new int();

        for (int i = 0; i < fishScore.Length; i++)
        {
            score = 0;
            fishScore[i].text = score.ToString();
        }
    }
    void catchedFishShow(string name)
    {
        Debug.Log(name);
        catchedFishPanel.SetActive(true);
        catchedFishPanel.GetComponent<Animator>().enabled = true;
        switch (name)
        {
            default:
            case "blue(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[0];
                catchedFishMessage.text = "CATCHED FISH : BLUE&YELLOW";
                soundManager.playFishCatchedSfx(0);
                break;
            case "Red(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[1];
                catchedFishMessage.text = "CATCHED FISH : WHITE&PURPLE";
                soundManager.playFishCatchedSfx(0);
                break;
            case "green(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[2];
                catchedFishMessage.text = "CATCHED FISH : ORANGE&GREEN";
                soundManager.playFishCatchedSfx(0);

                break;
            case "rimbowFish(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[3];
                catchedFishMessage.text = "CATCHED FISH : RAINBOW FISH";
                soundManager.playFishCatchedSfx(1);

                break;
            case "SawFish(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[4];
                destroyNet.Value = true;
                catchedFishMessage.text = "CATCHED FISH : SAW FISH";
                sawFishPanelWarn.SetActive(true);
                soundManager.playFishCatchedSfx(2);

                break;
        }
    }

}
