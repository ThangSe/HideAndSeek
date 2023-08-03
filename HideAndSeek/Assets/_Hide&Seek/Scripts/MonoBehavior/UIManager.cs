using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text _textTimer, _textScore, _gameOverTextScore, _gameEndText, _rewardText;
    [SerializeField] private GameObject _homePage, _tutorialPage, _gamePlayingPage, _gameOverPage, _backgroundPanel;
    [SerializeField] private Transform _textSpawnTransform;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioGameplay, _audioEatItem, _audioFinalPosAppear, _audioGameEnd, _audioCatched;

    private void Start()
    {
        MainManager.Instance.OnStateChanged += MainManager_OnStateChanged;
        MainManager.Instance.OnPopupSpawn += MainManager_OnPopupSpawn;
        MainManager.Instance.OnReceiveEffect += MainManager_OnReceiveEffect;
    }
    
    private void MainManager_OnReceiveEffect(object sender, System.EventArgs e)
    {
        _audioEatItem.Play();
    }

    private void MainManager_OnPopupSpawn(object sender, MainManager.OnPopupSpawnEventArgs e)
    {
        CreatePopup(e.popupList, e.textPopup, e.text, e.timerExisted, e.popupSpeed);
    }

    private void Update()
    {
        float timer = MainManager.Instance.GetPlayingTimer();

        if (MainManager.Instance.IsGamePlayingState())
        {
            _textScore.text = MainManager.Instance.GetCurrentScore().ToString();
            if (Mathf.RoundToInt(timer) < Mathf.CeilToInt(timer))
            {
                SetTimerText(timer);
            }
        }
    }

    private void SetTimerText(float timer)
    {
        if (timer >= 60f)
        {
            int minute = Mathf.RoundToInt(timer / 60);
            int second = Mathf.RoundToInt(timer - 60 * minute);
            if (second < 0)
            {
                minute--;
                second += 60;
            }
            if (second < 10)
            {
                _textTimer.text = minute + ":0" + second;
            }
            else
            {
                _textTimer.text = minute + ":" + second;
            }
        }
        if (timer < 60f && timer > 0f)
        {
            if (timer < 10)
            {
                _textTimer.text = "0:0" + Mathf.RoundToInt(timer);
            }
            else
            {
                _textTimer.text = "0:" + Mathf.RoundToInt(timer);
            }
        }
        if (timer <= 0f)
        {
            _textTimer.text = "0:00";
        }
    }

    private void MainManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (MainManager.Instance.IsMainMenuState())
        {
            _audioGameEnd.volume = 0f;
            _homePage.SetActive(true);
        }
        if (MainManager.Instance.IsWaitingToStartState())
        {
            _audioGameEnd.volume = 0f;
            _homePage.SetActive(false);
            _tutorialPage.SetActive(true);
            _gameOverPage.SetActive(false);
            _textScore.text = "0";
        }
        if (MainManager.Instance.IsCountdownToStartState())
        {
            SetTimerText(MainManager.Instance.GetPlayingTimer());
            _backgroundPanel.SetActive(false);
            _tutorialPage.SetActive(false);
            _gamePlayingPage.SetActive(true);
        }
        if (MainManager.Instance.IsGamePlayingState())
        {
            if (!MainManager.Instance.FinalPosAppeared())
            {
                _audioGameplay.time = 0;
                if (!_audioGameplay.isPlaying) _audioGameplay.Play();
                _audioGameplay.volume = 1f;
            }
        }
        if (MainManager.Instance.IsWarningEndPointState())
        {
            _audioGameplay.volume = 0;
            _audioFinalPosAppear.time = 0;
            if (!_audioFinalPosAppear.isPlaying) _audioFinalPosAppear.Play();
            _audioFinalPosAppear.volume = 1f;
        }
        if (MainManager.Instance.IsGameOverState())
        {
            _audioFinalPosAppear.volume = 0f;
            _audioGameplay.volume = 0f;
            if (MainManager.Instance.IsWinning())
            {
                _audioEatItem.Play();
            }
            else
            {
                _audioCatched.Play();
            }
        }
        if (MainManager.Instance.IsGameEndState())
        {
            _gamePlayingPage.SetActive(false);
            _backgroundPanel.SetActive(true);
            _gameOverPage.SetActive(true);
            _audioGameEnd.time = 0;
            if (!_audioGameEnd.isPlaying) _audioGameEnd.Play();
            _audioGameEnd.volume = 1f;
            if (MainManager.Instance.IsWinning())
            {
                _gameEndText.text = "YOU WIN";
                _rewardText.text = "You got a voucher";
            }
            else
            {
                _gameEndText.text = "YOU LOSE";
                _rewardText.text = "You have not received \n" + "any gift yet";
            }
            _gameOverTextScore.text = MainManager.Instance.GetCurrentScore().ToString();
        }
    }

    private void CreatePopup(List<IPopup> popupList, GameObject textPopup, string text, float timerExisted, float popupSpeed)
    {
        bool isAdded = false;
        for (int i = 0; i < popupList.Count; i++)
        {
            if (popupList[i].GetActiveState() == false)
            {
                popupList[i].Active(_textSpawnTransform.position, Vector3.zero, text, timerExisted, popupSpeed);
                isAdded = true;
                break;
            }
        }
        if (isAdded == false)
        {
            popupList.Add(Factory.CreatePopup(GameObject.Instantiate(textPopup, _gamePlayingPage.transform)));
            popupList[popupList.Count - 1].Active(_textSpawnTransform.position, Vector3.zero, text, timerExisted, popupSpeed);
        }
    }
}