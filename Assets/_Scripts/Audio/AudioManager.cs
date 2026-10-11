using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource audioPlaynormal;
    public AudioSource audioPlayBoss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnEnable()
    {
        EventBus<OnWaveChangedEvent>.Subscribe(OnWaveChanged);
    }

    private void OnDisable()
    {
        EventBus<OnWaveChangedEvent>.Unsubscribe(OnWaveChanged);
    }

    private void OnWaveChanged(OnWaveChangedEvent evt)
    {
        if (evt.IsBossWave)
            PlayBossBGM();
        else
            PlayNormalBGM();
    }

    public void PlayNormalBGM()
    {
        audioPlayBoss.Stop();
        if (!audioPlaynormal.isPlaying)
        {
            audioPlaynormal.Play();
        }
    }

    public void PlayBossBGM()
    {
        audioPlaynormal.Stop();
        if (!audioPlayBoss.isPlaying)
        {
            audioPlayBoss.Play();
        }
    }
}
