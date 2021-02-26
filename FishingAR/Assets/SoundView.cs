using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine.UI;


public class SoundView : MonoBehaviour
{
    [SerializeField] AudioSource CurrentActionSFX;
    [SerializeField] AudioSource FishSFX;
    [SerializeField] AudioSource FishJumpSFX;
    [SerializeField] AudioSource BackgroundSound;
    [SerializeField] AudioSource BackgroundMusic;
    [SerializeField] AudioSource UISFX;
    public AudioClip[] fishSFXCLips;
    public AudioClip[] BackgroundMusics;
    public AudioClip BackgroundSoundpond;
    public AudioClip Timer;
    public AudioClip effectCatch;
    public AudioClip qrScaned;
    public AudioClip gameStart;
    public AudioClip gameEnd;
    public AudioClip gameLose;
    public AudioClip buttonClick;
    public AudioClip[] fishJumpSfxClips;

    public void playFishCatchedSfx(int fish)
    {
        FishSFX.Stop();
        FishSFX.clip = fishSFXCLips[fish];
        FishSFX.Play();
    }
    public void playFishJump(int jumpMode)
    {
        FishJumpSFX.Stop();
        FishJumpSFX.clip = fishJumpSfxClips[jumpMode];
        FishJumpSFX.Play();
    }
    public void playBackgroundSoundSFX(AudioClip clip)
    {
        BackgroundSound.Stop();
        BackgroundSound.clip = clip;
        BackgroundSound.Play();
    }
    public void playCurrentActionSFX(AudioClip clip)
    {
        CurrentActionSFX.Stop();
        CurrentActionSFX.clip = clip;
        CurrentActionSFX.Play();
    }
    public void playUISFX()
    {
        UISFX.Stop();
        UISFX.clip = buttonClick;
        UISFX.Play();
    }
    public void playBackgroundMusic(AudioClip clip)
    {
        BackgroundMusic.Stop();
        BackgroundMusic.clip = clip;
        BackgroundMusic.Play();
    }
    public void resetSFX()
    {
        CurrentActionSFX.Stop();
        CurrentActionSFX.clip = null;
        FishSFX.Stop();
        FishSFX.clip = null;
        FishJumpSFX.Stop();
        FishJumpSFX.clip = null;
        BackgroundSound.Stop();
        BackgroundSound.clip = null;
        BackgroundMusic.Stop();
        BackgroundMusic.clip = null;
        UISFX.Stop();
        UISFX.clip = null;
    }
}