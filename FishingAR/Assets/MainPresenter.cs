using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class MainPresenter : MonoBehaviour
{
    [SerializeField] Text playerScore;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject leaderBoardPanel;
    [SerializeField] GameObject startTimerPanel;
    [SerializeField] GameObject scanCodePanel;
    [SerializeField] GameObject scorePanel;
    [SerializeField] GameObject[] allPanels;
    [SerializeField] GameObject inGameCanvas;
    [SerializeField] Image startTimerImage;
    [SerializeField] Sprite[] startTimerImageSprites;
    [SerializeField] Text TimerText;
    public string shareMsg;
    [SerializeField] TMP_InputField playerNameInputfield;

    [SerializeField] Button enterNameButton;
    [SerializeField] Button startGame;
    [SerializeField] Button leaderBoard;
    [SerializeField] Button scanCode;

    [SerializeField] Button[] backToMain;
    [SerializeField] Button reastartGame;
    [SerializeField] Camera[] gameCams;
    [SerializeField] FishingPresenter fishingPresneter;
    [SerializeField] GameObject spawnPrefab;
    [SerializeField] SoundView soundManager;
    [SerializeField] Button shareBtn;
    // Start is called before the first frame update
    void Start()
    {
        observeButtons();
        ObserveGameStatus();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ObserveGameStatus()
    {
        playerDataModel.currentGameStatus
           .Subscribe(proccedFishing)
           .AddTo(this);
        void proccedFishing(playerDataModel.GameStatus status)
        {
            switch (status)
            {
                case playerDataModel.GameStatus.OnStart:
                    InitializeUI();
                    soundManager.playBackgroundMusic(soundManager.BackgroundMusics[0]);
                   if (playerDataModel.playerLogged == true)
                    {
                        playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnMain;
                    }
                    break;
                case playerDataModel.GameStatus.OnMain:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_ => loginPanel.SetActive(false))
                        .Do(_ => leaderBoardPanel.SetActive(false))
                        .Do(_ => mainPanel.SetActive(true))
                        .Do(_ => soundManager.playBackgroundMusic(soundManager.BackgroundMusics[0]))
                        .Do(_ => setName(playerNameInputfield))
                        .Subscribe()
                        .AddTo(this);
                    break;
                case playerDataModel.GameStatus.OnLeaderBoard:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_ => scorePanel.SetActive(false))
                        .Do(_ => mainPanel.SetActive(false))
                        .Do(_ => leaderBoardPanel.SetActive(true))
                        .Subscribe()
                        .AddTo(this);
                    break;
                case playerDataModel.GameStatus.OnGameWaitToStart:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_ => mainPanel.SetActive(false))
                        .Do(_ => leaderBoardPanel.SetActive(false))
                        .Do(_=> scanCodePanel.SetActive(true))
                        .Do(_=> gameCams[1].gameObject.SetActive(true))
                        .Do(_ => gameCams[0].gameObject.SetActive(false))
                        .Subscribe()
                        .AddTo(this);
                    break;
                case playerDataModel.GameStatus.OnTimerStart:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_ => soundManager.playBackgroundSoundSFX(soundManager.BackgroundSoundpond))
                        .Do(_ => scanCodePanel.SetActive(false))
                        .Do(_ => startTimerPanel.SetActive(true))
                        .Do(_ => soundManager.playBackgroundMusic(soundManager.BackgroundMusics[1]))
                        .Do(_ => TimerText.text="READY")
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => soundManager.playCurrentActionSFX(soundManager.Timer))
                        .Do(_ => TimerText.text = "3")
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => TimerText.text = "2")
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => TimerText.text = "1")
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => TimerText.text = "GO")
                        .Do(_=>inGameCanvas.SetActive(true))
                        .Delay(TimeSpan.FromMilliseconds(1200))
                        .Do(_ => startTimerPanel.SetActive(false))
                        .Do(_ => soundManager.playCurrentActionSFX(soundManager.gameStart))
                        .Subscribe(_=> fishingPresneter.gameStart=true)
                        .AddTo(this);
                    break;
                case playerDataModel.GameStatus.OnTimeUp:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_=>inGameCanvas.SetActive(false))
                        .Do(_ => scorePanel.SetActive(true))
                        .Do(_=> playerScore.text="Score:"+playerDataModel.lastGameScore.ToString())
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => soundManager.playCurrentActionSFX(soundManager.gameEnd))
                        .Delay(TimeSpan.FromMilliseconds(2000))
                        .Do(_=>setScore())
                        .Do(_=> gameCams[1].transform.GetChild(0).gameObject.SetActive(false))
                        .Subscribe(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGameEnd)
                        .AddTo(this);
                    break;


            }
        }
    }
    void observeButtons()
    {
        enterNameButton.OnClickAsObservable()
            .Where(_ => playerNameInputfield.text != null)
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnMain)
            .Do(_ => playerDataModel.playerLogged=true)
            .Do(_=> soundManager.playUISFX())
            .Subscribe()
            .AddTo(this);
        startGame.OnClickAsObservable()
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGameWaitToStart)
            .Do(_ => soundManager.playUISFX())
            .Subscribe()
            .AddTo(this);
        leaderBoard.OnClickAsObservable()
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnLeaderBoard)
            .Do(_ => soundManager.playUISFX())
            .Subscribe()
            .AddTo(this);
        scanCode.OnClickAsObservable()
            .Do(_ => soundManager.playUISFX())
            .Where(_ => GameObject.FindGameObjectWithTag("playGroundSpawnPos") != null)
            .Do(_ => Debug.Log("scaned"))
            .Delay(TimeSpan.FromMilliseconds(1000))
            .Do(_ => spawnGround(GameObject.FindGameObjectWithTag("playGroundSpawnPos").transform, spawnPrefab))
            .Do(_ => gameCams[1].transform.GetChild(0).gameObject.SetActive(true))
            .Do(_ => soundManager.playCurrentActionSFX(soundManager.qrScaned))
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimerStart)
            .Subscribe(_=> playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimerStart)
            .AddTo(this);
        reastartGame.OnClickAsObservable()
            .Where(_ => fishingPresneter.gameStart != true)
            .Do(_ => soundManager.playUISFX())
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGameWaitToStart)
            .Subscribe(_=> restartScene())
            .AddTo(this);
        for(int i = 0; i < backToMain.Length; i++)
        {
            backToMain[i].OnClickAsObservable()
            .Do(_ => InitializeUI())
            .Do(_=>setScore())
            .Do(_ => soundManager.playUISFX())
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnMain)
            .Subscribe(_ => restartScene())
            .AddTo(this);
        }
        shareBtn.OnClickAsObservable()
            .Do(_ => sharefunc())
            .Subscribe()
            .AddTo(this);
        
    }
    void InitializeUI()
    {
        inGameCanvas.SetActive(false);
        fishingPresneter.gameStart = false;
        gameCams[1].transform.GetChild(0).gameObject.SetActive(false);
        gameCams[1].gameObject.SetActive(false);
        gameCams[0].gameObject.SetActive(true);
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
        leaderBoardPanel.SetActive(false);
        playerDataModel.lastGameScore = 0;
        if (playerDataModel.playerLogged == false)
        {
            playerDataModel.playerName = "player";

        }

    }
    void restartScene()
    {
        soundManager.resetSFX();
        gameCams[1].transform.GetChild(0).gameObject.SetActive(false);
        fishingPresneter.Timer = 60;
        for(int i=0;i< allPanels.Length; i++)
        {
            if(allPanels[i].name== "MainMenu")
            {
                allPanels[i].SetActive(true);
            }
            else
            {
                allPanels[i].SetActive(false);

            }
        }
        if (FindObjectOfType<PlayGroundManager>() != null)
        {
            GameObject playGround = FindObjectOfType<PlayGroundManager>().gameObject;
            Destroy(playGround);
        }
        fishingPresneter.resetScore();
    }
    void setScore()
    {
        playerDataModel.lastGameScore = 0;
        for (int i = 0; i < fishingPresneter.fishScore.Length; i++)
        {
            playerDataModel.lastGameScore += int.Parse(fishingPresneter.fishScore[i].text);
        }
        if(playerDataModel.currentGameStatus.Value == playerDataModel.GameStatus.OnGameEnd)
        {
            DatabaseManager._instance.getScore(FetchedScore =>
            {
                DatabaseManager._instance.setScore(playerDataModel.lastGameScore + FetchedScore);
            });       
        }
        playerScore.text = "Score:" + playerDataModel.lastGameScore.ToString();
    }
    void setName(TMP_InputField playername) 
    {
        playerDataModel.playerName = playername.text;
        // userName saving is handled in the DatabaseManager Script

    }
    void spawnGround(Transform spawnPos , GameObject prefab)
    {
        Vector3 offset = new Vector3(spawnPos.position.x-2.5f,spawnPos.position.y-0.7f,spawnPos.position.z-1f);
        Instantiate(spawnPrefab, offset, spawnPrefab.transform.rotation, transform);
    }
    void sharefunc()
    {
        shareMsg = "Its amazin to catch fish in AR , in the AR fishing compitition I get this Score : " + playerDataModel.lastGameScore + " Points";
        StartCoroutine(TakeScreenshotAndShare());
    }
    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, ss.EncodeToPNG());

        // To avoid memory leaks
        Destroy(ss);

        new NativeShare().AddFile(filePath)
            .SetSubject("Subject goes here").SetText("Hello world!")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();

        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
    }
}
