using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Fire Alarm Effect Reference")]
    public FireAlarmScript fireAlarmScript;

    [Header("Timing Settings")]
    [Tooltip("Minimum seconds between fire alarm activations")]
    public float minDelay = 8f;

    [Tooltip("Maximum seconds between fire alarm activations")]
    public float maxDelay = 20f;

    [Tooltip("How long the alarm stays active before turning off")]
    public float alarmDuration = 20f;

    private bool isRunning = true;

    void Start()
    {
        if (fireAlarmScript == null)
        {
            Debug.LogWarning("RandomEffectManager: FireAlarmEffect not assigned!");
            return;
        }

        StartCoroutine(AlarmRoutine());
    }

    IEnumerator AlarmRoutine()
    {
        while (isRunning)
        {
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            fireAlarmScript.ActivateAlarm();
            Debug.Log("Fire alarm activated!");

            yield return new WaitForSeconds(alarmDuration);

            // Change this to run when switch is activated
            fireAlarmScript.DeactivateAlarm();
            Debug.Log("Fire alarm deactivated!");
        }
    }

    public void StopRandomEffects()
    {
        isRunning = false;
        StopAllCoroutines();
        fireAlarmScript.DeactivateAlarm();
    }
}
