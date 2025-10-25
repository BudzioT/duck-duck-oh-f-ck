using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class FireAlarmScript : MonoBehaviour
{
    public AudioClip[] startSounds;
    private AudioSource audioSource;
    public AudioSource alarmAudio;

    public Image overlayImage;
    public float pulseSpeed = 2f;
    public float maxAlpha = 0.4f;
    public float minAlpha = 0.1f;

    private bool isActive = false;
    private float pulseTimer = 0f;

    public FireEmojiManager emojiManager;

    void Start()
    {
        if (overlayImage == null)
            overlayImage = GetComponent<Image>();

        SetAlpha(0f);
    }

    void Update()
    {
        if (!isActive) return;

        pulseTimer += Time.deltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(pulseTimer) + 1f) / 2f);
        SetAlpha(alpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }

    public void ActivateAlarm()
    {
        isActive = true;

        if (startSounds != null && startSounds.Length > 0)
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            AudioClip clip = startSounds[Random.Range(0, startSounds.Length)];
            audioSource.PlayOneShot(clip); // plays once without looping
        }

        if (alarmAudio != null)
        {
            alarmAudio.loop = true;
            alarmAudio.Play();
        }

        emojiManager.Activate();
    }

    public void DeactivateAlarm()
    {
        isActive = false;
        SetAlpha(0f);

        if (alarmAudio != null)
        {
            alarmAudio.Stop();
        }
        
        emojiManager.Deactivate();
    }
}
