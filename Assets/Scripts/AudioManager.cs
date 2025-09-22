using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mode & Debug")]
    [Tooltip("Tick in Level_Manual. Untick in StartScene/menu.")]
    public bool isGameScene = true;
    public bool logMessages = false;

    [Header("BGM (AudioSources)")]
    public AudioSource startScene;         // Menu/start scene loop
    public AudioSource game_bgm;           // Main gameplay loop
    public AudioSource game_intro;         // Short intro before game_bgm
    public AudioSource scared_ghost_bgm;   // Vulnerable-ghost music
    public AudioSource power_up_bgm;       // Optional background during power-up

    [Header("SFX (AudioSources)")]
    public AudioSource ghost_death_sfx;
    public AudioSource move_sfx;
    public AudioSource pellet_eat_sfx;
    public AudioSource player_death_sfx;
    public AudioSource power_up_sfx;
    public AudioSource wall_bump_sfx;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure nothing auto-plays by accident; we’ll start tracks via code.
        SafeOff(startScene);
        SafeOff(game_bgm);
        SafeOff(game_intro);
        SafeOff(scared_ghost_bgm);
        SafeOff(power_up_bgm);
    }

    void Start()
    {
        if (isGameScene)
        {
            // Level_Manual requirement: intro (~3s or clip length, whichever is earlier) → game loop
            StartCoroutine(PlayIntroThenGame());
        }
        else
        {
            // Menu/Start scene
            PlayStartSceneBGM();
        }
    }

    // ---------------- BGM control ----------------

    public IEnumerator PlayIntroThenGame()
    {
        StopAllBGM();

        if (game_intro != null)
        {
            if (logMessages) Debug.Log("[Audio] Playing game_intro");
            game_intro.Play();

            float wait = 3f;
            if (game_intro.clip != null) wait = Mathf.Min(3f, game_intro.clip.length);
            yield return new WaitForSeconds(wait);
        }

        if (game_bgm != null)
        {
            if (logMessages) Debug.Log("[Audio] Switching to game_bgm");
            game_bgm.loop = true;
            game_bgm.Play();
        }
        else if (logMessages) Debug.LogWarning("[Audio] game_bgm is not assigned.");
    }

    public void PlayStartSceneBGM()
    {
        StopAllBGM();
        if (startScene != null)
        {
            if (logMessages) Debug.Log("[Audio] Playing startScene BGM");
            startScene.loop = true;
            startScene.Play();
        }
        else if (logMessages) Debug.LogWarning("[Audio] startScene is not assigned.");
    }

    public void EnterScaredStateBGM()
    {
        PlayPowerUpSFX();
        StopAllBGM();
        if (scared_ghost_bgm != null) scared_ghost_bgm.Play();
        if (power_up_bgm != null) power_up_bgm.Play();   // optional layer if you want it
    }

    public void ReturnToNormalBGM()
    {
        StopAllBGM();
        if (game_bgm != null) game_bgm.Play();
    }

    private void StopAllBGM()
    {
        SafeStop(startScene);
        SafeStop(game_bgm);
        SafeStop(game_intro);
        SafeStop(scared_ghost_bgm);
        SafeStop(power_up_bgm);
    }

    // ---------------- SFX helpers ----------------

    public void PlayPelletSFX()      { SafePlay(pellet_eat_sfx); }
    public void PlayMoveSFX()        { SafePlay(move_sfx); }
    public void PlayWallBumpSFX()    { SafePlay(wall_bump_sfx); }
    public void PlayPowerUpSFX()     { SafePlay(power_up_sfx); }
    public void PlayGhostDeathSFX()  { SafePlay(ghost_death_sfx); }
    public void PlayPlayerDeathSFX() { SafePlay(player_death_sfx); }

    // ---------------- Utils ----------------

    private static void SafeOff(AudioSource src)
    {
        if (!src) return;
        src.playOnAwake = false;
        src.spatialBlend = 0f; // force 2D
    }

    private static void SafeStop(AudioSource src)
    {
        if (src && src.isPlaying) src.Stop();
    }

    private static void SafePlay(AudioSource src)
    {
        if (src) src.Play();
    }
}
