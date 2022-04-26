using UnityEngine;

public enum Sound { Click, Swipe, Boom, Achieve }

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    //[NamedArray(new string[] { "Click", "Swipe", "Boom", "Achieve" })]
    [SerializeField] AudioClip[] sounds;

    AudioSource audioSource;
    bool mute;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Messenger<Sound>.AddListener(GameEvent.ON_SOUND_PLAY, OnSoundPlay);
    }

    private void OnDestroy()
    {
        Messenger<Sound>.RemoveListener(GameEvent.ON_SOUND_PLAY, OnSoundPlay);
    }

    void OnSoundPlay(Sound sound)
    {
        if (!mute && PlayerData.Sound)
        {
            audioSource.clip = sounds[(int)sound];
            audioSource.Play();
        }
    }

    public bool Mute
    {
        get
        {
            return mute;
        }
        set
        {
            mute = value;
        }
    }
}
