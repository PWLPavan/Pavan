using UnityEngine;
using System.Collections;
using FGUnity.Utils;

public class SoundManager : SingletonBehavior<SoundManager>
{
    #region Inspector

    [Header("Audio Players")]
    public AudioSource ambient;
    public AudioSource music;
    public AudioSource musicFX;
    public AudioSource sfx;
    public AudioSource vo;
    
    [Header("Car Sounds")]
    public AudioClip carEnter;
    public AudioClip carExit;
    public AudioClip carIdle;

    [Header("Chicken Sounds")]
    public AudioClip[] chickenDrag;
    public AudioClip chickenHappy;
    public AudioClip chickenHover;
    public AudioClip chickenSnapBack;
    public AudioClip chickenSelect;
    public AudioClip[] chickenCount;
    public AudioClip[] chickenAngryCount;
    public AudioClip[] nestCount;
    public AudioClip chickenConvertToOnes;
    public AudioClip chickenConvertToTens;
    public AudioClip chickenCorrect1;
    public AudioClip chickenCorrect2;
    public AudioClip chickenCorrect3;
    public AudioClip chickenSeatbeltBuckle;
    public AudioClip chickenSeatbeltStretch;
    public AudioClip chickenSeatbeltImpact;
    public AudioClip[] chickenSubtractZone;

    [Header("UI Sounds")]
    public AudioClip buttonClick;
    
    public AudioClip counterUpdates;
    public AudioClip numberHighlight;

    public AudioClip planeEnter;
    public AudioClip planeExit;

    public AudioClip polaroidEnter;
    public AudioClip polaroidExit;

    public AudioClip correctAnswer;
    public AudioClip incorrectAnswer;

    public AudioClip[] hudTapEgg;

    [Header("Kiwi Sounds")]
    public AudioClip kiwiChirp;
    public AudioClip[] kiwiCount;

    [Header("Pilot Sounds")]
    public AudioClip pilotHappy;
    public AudioClip pilotSad;
    public AudioClip pilotSayProblem;
    public AudioClip pilotFixed;
    public AudioClip pilotAngry;

    [Header("Award Sounds")]
    public AudioClip eggSpawn;
    public AudioClip eggFly;
    public AudioClip[] eggEarn;
    public AudioClip eggLose;

    public AudioClip eggMeterTop;
    public AudioClip eggMeterOpen;
    public AudioClip eggMeterClose;
    public AudioClip eggMeterClear;
    
    public AudioClip stampEarn;
    public AudioClip[] stampLift;
    public AudioClip[] stampDrop;
    public AudioClip stampControlsOn;
    public AudioClip[] stampControlsTouch;
    public AudioClip[] stampSlide;

    public AudioClip suitcaseOpen;
    public AudioClip suitcaseClose;

    [Header("Music and Ambience")]
    public AudioClip gameAmbience;
    public AudioClip gameMusic;

    [Header("Minigame")]

    public AudioClip minigameReveal;
    public AudioClip minigamePenalty;

    public AudioClip minigameMusic;
    public AudioClip minigameStinger;
    public AudioClip minigamePanic;

    public AudioClip minigameDropNest;
    public AudioClip minigameDropEmpty;

    [Header("Transition Timing")]
    public float TransitionTime = 1.0f;
    public float DuckVolume = 0f;
    public float DuckTime = 1f;
    #endregion

    #region Behavior

    protected override void Awake()
    {
 	    base.Awake();
        SaveData.CreateSingleton();
    }

    #endregion

    #region Methods

    public void PlayOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null)
            return;
        if (sfx == null)
            return;
        sfx.PlayOneShot(clip, volume);
    }

    public void PlayRandomOneShot(AudioClip[] possibleClips, float volume = 1.0f)
    {
        if (possibleClips == null)
            return;
        if (possibleClips.Length <= 0)
            return;
        PlayOneShot(possibleClips[Random.Range(0, possibleClips.Length)], volume);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;
        if (music == null)
            return;
        if (music.isPlaying && music.clip == clip)
            return;
        music.clip = clip;
        music.loop = true;
        UpdateVolume();
        music.Play();
    }

    public void PlayMusicOneShot(AudioClip clip, float inVolume = 1.0f)
    {
        if (clip == null)
            return;
        if (musicFX == null)
            return;

        musicFX.PlayOneShot(clip, inVolume);
    }

    public CoroutineHandle PlayMusicTransition(AudioClip inNewMusic, float inTransitionTime)
    {
        if (music == null)
            return CoroutineHandle.Null;
        if (music.isPlaying && music.clip == inNewMusic)
            return CoroutineHandle.Null;

        m_MusicTransitionCoroutine.Clear();
        if (music.clip == null)
        {
            m_MusicVolume = 0.0f;
            PlayMusic(inNewMusic);
            m_MusicTransitionCoroutine = this.SmartCoroutine(FadeInMusicRoutine(inTransitionTime));
        }
        else if (inNewMusic == null)
        {
            m_MusicTransitionCoroutine = this.SmartCoroutine(FadeOutMusicRoutine(inTransitionTime));
        }
        else
        {
            m_MusicTransitionCoroutine = this.SmartCoroutine(FadeSwitchMusicRoutine(inNewMusic, inTransitionTime));
        }

        return m_MusicTransitionCoroutine;
    }

    public void DuckMusic()
    {
        DuckMusic(DuckVolume, DuckTime);
    }

    public void DuckMusic(float inVolume, float inTime)
    {
        m_MusicDuckCoroutine.Clear();
        m_MusicDuckCoroutine = this.SmartCoroutine(DuckMusicRoutine(inVolume, inTime));
    }

    public void PauseMusic()
    {
        if (music)
            music.Pause();
    }

    public void StopMusic()
    {
        if (music)
            music.Stop();
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null)
            return;
        if (ambient == null)
            return;
        ambient.clip = clip;
        ambient.Play ();
    }

    public void PauseAmbience()
    {
        if (ambient)
            ambient.Pause();
    }
    
    public void StopAmbience()
    {
        if (ambient)
            ambient.Stop();
    }

    public void UpdateMute()
    {
        if (ambient != null)
            ambient.mute = SaveData.instance.MuteSound;
        if (sfx != null)
            sfx.mute = SaveData.instance.MuteSound;
        if (vo != null)
            vo.mute = SaveData.instance.MuteSound;
        if (music != null)
            music.mute = SaveData.instance.MuteMusic;
        if (musicFX != null)
            musicFX.mute = SaveData.instance.MuteMusic;
    }

    public void UpdateVolume()
    {
        if (music != null)
            music.volume = m_MusicVolume * m_MusicDuckVolume;
    }

    #endregion

    #region Music Transition Periods

    private float m_MusicDuckVolume = 1.0f;
    private float m_MusicVolume = 1.0f;

    private CoroutineHandle m_MusicTransitionCoroutine;
    private CoroutineHandle m_MusicDuckCoroutine;

    private IEnumerator FadeSwitchMusicRoutine(AudioClip inNewMusic, float inTotalTime)
    {
        yield return FadeOutMusicRoutine(inTotalTime / 2);
        PlayMusic(inNewMusic);
        yield return FadeInMusicRoutine(inTotalTime / 2);
    }

    private IEnumerator FadeOutMusicRoutine(float inTime)
    {
        float time = 0;

        float initialVolume = m_MusicVolume;

        while(time < inTime)
        {
            yield return null;
            time += Time.deltaTime;
            m_MusicVolume = Mathf.Lerp(initialVolume, 0.0f, time / inTime);
            UpdateVolume();
        }

        StopMusic();
    }

    private IEnumerator FadeInMusicRoutine(float inTime)
    {
        float time = 0;

        float initialVolume = m_MusicVolume;

        while (time < inTime)
        {
            yield return null;
            time += Time.deltaTime;
            m_MusicVolume = Mathf.Lerp(initialVolume, 1.0f, time / inTime);
            UpdateVolume();
        }
    }

    private IEnumerator DuckMusicRoutine(float inTargetVolume, float inTime)
    {
        float time = 0;
        float transitionTime = inTime / 8;
        float duckTime = inTime - (transitionTime * 2);

        float initialDucking = m_MusicDuckVolume;

        while(time < transitionTime)
        {
            yield return null;
            time += Time.deltaTime;
            m_MusicDuckVolume = Mathf.Lerp(initialDucking, inTargetVolume, time / transitionTime);
            UpdateVolume();
        }

        yield return duckTime;

        time = 0;
        while (time < transitionTime)
        {
            yield return null;
            time += Time.deltaTime;
            m_MusicDuckVolume = Mathf.Lerp(inTargetVolume, 1.0f, time / transitionTime);
            UpdateVolume();
        }
    }

    #endregion
}
