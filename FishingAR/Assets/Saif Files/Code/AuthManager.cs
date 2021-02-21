using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using TMPro;

public class AuthManager : MonoBehaviour
{
    public static AuthManager _instance;
    FirebaseAuth auth;
    public TMP_InputField _userNameText;
    public GameObject LoginPanel;
    public GameObject MainMenuPanel;


    private void Awake()
    {
        _instance = this;
        auth = FirebaseAuth.DefaultInstance;
    }
    private void Start()
    {
        if (IsLoggedIn())
        {
            LoginPanel.SetActive(false);
            MainMenuPanel.SetActive(true);
        }
        else
        {
            LoginPanel.SetActive(true);
            MainMenuPanel.SetActive(false);
        }
    }
    public void SignIn()
    {
        SignInAnonymously((userID) =>
        {
            DatabaseManager._instance.CreateUser(userID,_userNameText.text, 0);
        });
    }
    public bool IsLoggedIn()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
            return true;
        else
            return false;
    }
    public void SignInAnonymously(Action<string> Success)
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Success(newUser.UserId);
        });

        //Call database manager to createUser
    }
}
