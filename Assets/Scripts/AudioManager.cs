using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }

    public AudioSource soundEffectSource; 
    public AudioClip[] soundEffects; 

    public void PlaySoundById(int id)
    {
        if (id >= 0 && id < soundEffects.Length)
        {
            soundEffectSource.clip = soundEffects[id];
            soundEffectSource.Play();
        }
        else
        {
            Debug.LogWarning("Sound ID out of range: " + id);
        }
    }

    public void PlayHarpoonSound()
    {
        PlaySoundById(0); // Assuming the harpoon sound is at index 0
    }

    public void PlayBubbleSound()
    {
        PlaySoundById(1); // Assuming the bubble sound is at index 1
    }
}
