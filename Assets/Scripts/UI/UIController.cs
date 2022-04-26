using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioController))]
public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject shop;
    [SerializeField] private TextMeshProUGUI coins;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private TMP_InputField tapDist;


    AudioController audioController;

    private void Awake()
    {
        Initialize();
        CloseAllWindows();
    }

    private void CloseAllWindows()
    {
        if (settings != null) settings.SetActive(false);
        if (shop != null) shop.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);
    }

    private void Initialize()
    {
        audioController = GetComponent<AudioController>();

        if (settings != null)
        {
            Toggle[] settingsToggles = settings.GetComponentsInChildren<Toggle>();
            audioController.Mute = true;
            for (int i = 0; i < settingsToggles.Length; i++)
            {
                switch (settingsToggles[i].name)
                {
                    case "MusicToggle": settingsToggles[i].isOn = PlayerData.Music; break;
                    case "SoundToggle": settingsToggles[i].isOn = PlayerData.Sound; break;
                    case "CollisionToggle": settingsToggles[i].isOn = PlayerPrefs.GetInt("Collision", 1) > 0; break;
                }                
            }
            audioController.Mute = false;
        }

        if (coins != null)
        {
            Messenger.AddListener(GameEvent.ON_COIN_VALUE, OnCoinValue);
            coins.text = "<sprite index=0>" + PlayerData.Coins.ToString();
        }

        if (tapDist != null)
        {
            tapDist.text = PlayerPrefs.GetFloat("TapDistError", 30f).ToString();
        }
    }

    private void OnDestroy()
    {
        if (coins != null)
        {
            Messenger.RemoveListener(GameEvent.ON_COIN_VALUE, OnCoinValue);
        }
    }

    #region Main

    public void OnStartPlay()
    {
        SceneManager.LoadScene(PlayerData.GameTheme);
        PlaySound(Sound.Click);
    }
    public void OnOpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        PlaySound(Sound.Click);
    }

    #endregion

    #region Settings

    public void OnOpenSettings()
    {
        CloseAllWindows();
        if (settings != null)
        {
            settings.SetActive(true);
        }
        PlaySound(Sound.Click);
    }
    public void OnSoundValue(Toggle soundToggle)
    {
        PlayerData.Sound = soundToggle.isOn;
        PlaySound(Sound.Click);
    }
    public void OnMusicValue(Toggle musicToggle)
    {
        PlayerData.Music = musicToggle.isOn;
        PlaySound(Sound.Click);
    }
    public void OnCollisionValue(Toggle collisionToggle)
    {
        PlayerPrefs.SetInt("Collision", collisionToggle.isOn ? 1 : 0);
        PlaySound(Sound.Click);
    }
    public void OnTapDistValue()
    {
        PlayerPrefs.SetFloat("TapDistError", float.Parse(tapDist.text));
        PlaySound(Sound.Click);
    }
    public void OnCloseSettings()
    {
        if (settings != null)
        {
            PlaySound(Sound.Click);
            settings.SetActive(false);
        }
    }

    #endregion

    #region Shop

    private GameObject[] shopItems;
    private bool initShop;

    public void OnOpenShop()
    {
        CloseAllWindows();
        if (shop != null)
        {
            PlaySound(Sound.Click);
            shop.SetActive(true);

            if (!initShop)
            {
                shopItems = GameObject.FindGameObjectsWithTag("ShopItem");
                initShop = true;
            }

            for (int i = 0; i < shopItems.Length; i++)
            {
                if (shopItems[i].name != PlayerData.GameTheme)
                {
                    shopItems[i].SetActive(false);
                }
                else
                {
                    shopItems[i].SetActive(true);
                }
            }
        }
    }
    public void OnSelectTheme(string gameThemeName)
    {
        PlayerData.GameTheme = gameThemeName;
        PlaySound(Sound.Achieve);
        Messenger<string>.Broadcast(GameEvent.THEME_CHANGED, gameThemeName);

        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i].name != gameThemeName)
            {
                shopItems[i].SetActive(false);
            }
            else
            {
                shopItems[i].SetActive(true);
            }
        }
    }
    public void OnCloseShop()
    {
        if (shop != null)
        {
            shop.SetActive(false);
        }
    }

    #endregion

    #region PauseMenu

    public void OnOpenPauseMenu()
    {
        CloseAllWindows();
        if (pauseMenu != null)
        {
            PlaySound(Sound.Click);
            Messenger<bool>.Broadcast(GameEvent.ON_PAUSE, true);
            pauseMenu.SetActive(true);
        }
    }
    public void OnClosePauseMenu()
    {
        if (pauseMenu != null)
        {
            PlaySound(Sound.Click);
            Messenger<bool>.Broadcast(GameEvent.ON_PAUSE, false);
            pauseMenu.SetActive(false);            
        }        
    }

    #endregion

    #region GameUI

    public void OnCoinValue()
    {
        if (coins != null)
        {
            coins.text = "<sprite index=0>" + PlayerData.Coins.ToString();
        }
    }

    #endregion

    void PlaySound(Sound sound)
    {
        if (!audioController.Mute)
        {
            Messenger<Sound>.Broadcast(GameEvent.ON_SOUND_PLAY, sound);
        }
    }
}
