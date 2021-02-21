using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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

    [SerializeField] Image startTimerImage;
    [SerializeField] Sprite[] startTimerImageSprites;

    [SerializeField] TMP_InputField playerNameInputfield;

    [SerializeField] Button enterNameButton;
    [SerializeField] Button startGame;
    [SerializeField] Button leaderBoard;
    [SerializeField] Button scanCode;

    [SerializeField] Button[] backToMain;
    [SerializeField] Button reastartGame;
    [SerializeField] Camera[] gameCams;
    [SerializeField] FishingPresenter fishingPresneter;

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
                        .Do(_ => scanCodePanel.SetActive(false))
                        .Do(_ => startTimerPanel.SetActive(true))
                        .Do(_ => startTimerImage.sprite = startTimerImageSprites[0])
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_=> startTimerImage.sprite= startTimerImageSprites[1])
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => startTimerImage.sprite = startTimerImageSprites[2])
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => startTimerImage.sprite = startTimerImageSprites[3])
                        .Delay(TimeSpan.FromMilliseconds(1000))
                        .Do(_ => startTimerImage.sprite = startTimerImageSprites[4])
                        .Delay(TimeSpan.FromMilliseconds(400))
                        .Do(_ => startTimerPanel.SetActive(false))
                        .Subscribe(_=> fishingPresneter.gameStart=true)
                        .AddTo(this);
                    break;
                case playerDataModel.GameStatus.OnTimeUp:
                    Observable.Timer(TimeSpan.Zero)
                        .Do(_ => scorePanel.SetActive(true))
                        .Do(_=> playerScore.text="Score:"+playerDataModel.lastGameScore.ToString())
                        .Delay(TimeSpan.FromMilliseconds(3000))
                        .Do(_=>setScore())
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
            .Subscribe()
            .AddTo(this);
        startGame.OnClickAsObservable()
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGameWaitToStart)
            .Subscribe()
            .AddTo(this);
        leaderBoard.OnClickAsObservable()
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnLeaderBoard)
            .Subscribe()
            .AddTo(this);
        scanCode.OnClickAsObservable()
            .Where(_ => FindObjectOfType<PlayGroundManager>()!= null)
            .Do(_=>Debug.Log("scaned"))
            .Delay(TimeSpan.FromMilliseconds(1000))
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimerStart)
            .Subscribe(_=> playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimerStart)
            .AddTo(this);
        reastartGame.OnClickAsObservable()
            .Where(_ => fishingPresneter.gameStart != true)
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGameWaitToStart)
            .Subscribe(_=> restartScene())
            .AddTo(this);
        for(int i = 0; i < backToMain.Length; i++)
        {
            backToMain[i].OnClickAsObservable()
            .Where(_ => fishingPresneter.gameStart != true)
            .Do(_ => InitializeUI())
            .Do(_ => playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnMain)
            .Subscribe(_ => restartScene())
            .AddTo(this);
        }
        
    }
    void InitializeUI()
    {
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
        string name = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(name);
    }
    void setScore()
    {
        for (int i = 0; i < fishingPresneter.fishScore.Length; i++)
        {
            playerDataModel.lastGameScore += int.Parse(fishingPresneter.fishScore[i].text);
        }
        DatabaseManager._instance.setScore(playerDataModel.lastGameScore);
    }

    void setName(TMP_InputField playername) 
    {
        playerDataModel.playerName = playername.text;
        // userName saving is handled in the DatabaseManager Script

    }
}
