using UnityEngine;

public static class PlayerData
{
    public static int Coins
    {
        get
        {
            return PlayerPrefs.GetInt("Coins", 0);
        }
        set
        {
            PlayerPrefs.SetInt("Coins", value);
        }
    }

    public static bool Sound
    {
        get
        {
            return PlayerPrefs.GetInt("Sound", 1) > 0;
        }
        set
        {
            PlayerPrefs.SetInt("Sound", value ? 1 : 0);
        }
    }

    public static bool Music
    {
        get
        {
            return PlayerPrefs.GetInt("Music", 1) > 0;
        }
        set
        {
            PlayerPrefs.SetInt("Music", value ? 1 : 0);
        }
    }

    public static string GameTheme
    {
        get
        {
            return PlayerPrefs.GetString("GameTheme", "Standart");
        }
        set
        {
            PlayerPrefs.SetString("GameTheme", value);
        }
    }
}
