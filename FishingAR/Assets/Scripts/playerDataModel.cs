using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UniRx;
[Serializable]
public struct playerDataModel
{


	public static string playerName;
    public static bool playerLogged;

    public static int lastGameScore;
    public enum GameStatus
    {
        OnStart,
        OnEnterName,
        OnMain,
        OnLeaderBoard,
        OnGameWaitToStart,
        OnTimerStart,
        OnGame,
        OnTimeUp,
        OnGameEnd,
    }
    public static ReactiveProperty<GameStatus> currentGameStatus = new ReactiveProperty<GameStatus>(GameStatus.OnStart);

    public override string ToString()
	{
		return JsonUtility.ToJson(this, true);
	}

}
