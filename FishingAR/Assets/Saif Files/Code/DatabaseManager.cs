using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase;
using System;
using Firebase.Auth;

[Serializable]
public class User
{
    public string UserName;
    public int UserScore;
    public User(string _UserName, int _UserScore)
    {
        UserName = _UserName;
        UserScore = _UserScore;
    }
}
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager _instance;
    DatabaseReference reference;

    private void Awake()
    {
        _instance = this;
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri("https://fishingar-17fed-default-rtdb.firebaseio.com");
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateUser(string _UserID, string _UserName, int _UserScore)
    {
        User _User = new User(_UserName, _UserScore);
        string json_User = JsonUtility.ToJson(_User);
        reference.Child("users").Child(_UserID).SetRawJsonValueAsync(json_User);
        Debug.Log("User Created...");
    }
    public void setScore(int Score)
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        FirebaseDatabase
            .DefaultInstance
            .RootReference
            .Child("users")
            .Child(user.UserId)
            .Child("UserScore")
            .SetValueAsync(Score);

    }
}
