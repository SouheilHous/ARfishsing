using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System;
using UnityEngine.UI;


[Serializable]
public class LeaderboardRanking
{
    public List<UserRank> _users;
}

[Serializable]
public class UserRank
{
    public int Rank;
    public string UserName;
    public int UserScore;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager _instance;
    public string LEADERBOARD_GET;
    public int topUsers;
    public Transform leaderboardParent;
    public GameObject rankElementPrefab;
    List<UserRank> rankingUsers;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        //fetchLeaderboard();
    }

    public void fetchLeaderboard()
    {
        GetUsersWithTopScores(topUsers, users =>
         {
             rankingUsers = new List<UserRank>(users);
             setupLeaderboard();
         });
    }


    public void GetUsersWithTopScores(int numberOfUsersToReturn, Action<List<UserRank>> success)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(LEADERBOARD_GET + "?number_of_results={0}", numberOfUsersToReturn));
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            jsonResponse = "{\"_users\":" + jsonResponse.ToString() + "}";
            LeaderboardRanking userRankings = JsonUtility.FromJson<LeaderboardRanking>(jsonResponse);
            success(userRankings._users);
        }
        else
            success(new List<UserRank>());
    }
    public void setupLeaderboard()
    {
       

        foreach (UserRank _user in rankingUsers)
        {
            GameObject _element = Instantiate(rankElementPrefab, leaderboardParent);
            _element.GetComponent<RankElement>().setUserData(_user.Rank, _user.UserName, _user.UserScore);
        }
    }
}
