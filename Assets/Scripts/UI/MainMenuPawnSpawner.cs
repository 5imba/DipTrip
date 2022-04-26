using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPawnSpawner : MonoBehaviour
{
    [SerializeField] Transform standartPawn;
    [SerializeField] Transform spacePawn;

    Transform pawn;

    void SetPawn(string gameThemeName)
    {
        switch (gameThemeName)
        {
            case "Standart":
                pawn = Instantiate(standartPawn);
                pawn.parent = transform;
                pawn.localRotation = Quaternion.identity;
                break;
            case "Space":
                pawn = Instantiate(spacePawn);
                pawn.parent = transform;
                pawn.localRotation = Quaternion.identity;
                break;
        }
    }

    void OnThemeChanged(string gameThemeName)
    {
        if (pawn != null)
        {
            Destroy(pawn.gameObject);
        }

        SetPawn(gameThemeName);
    }

    void Start()
    {
        SetPawn(PlayerData.GameTheme);

        Messenger<string>.AddListener(GameEvent.THEME_CHANGED, OnThemeChanged);
    }

    private void OnDestroy()
    {
        Messenger<string>.RemoveListener(GameEvent.THEME_CHANGED, OnThemeChanged);
    }
}
