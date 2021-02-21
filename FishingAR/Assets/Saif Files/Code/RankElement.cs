using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankElement : MonoBehaviour
{
    public TextMeshProUGUI userRank;
    public TextMeshProUGUI userName;
    public TextMeshProUGUI userScore;


    public void setUserData(int _rank, string _name, int _score)
    {
        userRank.text = "" + _rank;
        userName.text = "" + _name;
        userScore.text = "" + _score + " PTS";
    }
}
