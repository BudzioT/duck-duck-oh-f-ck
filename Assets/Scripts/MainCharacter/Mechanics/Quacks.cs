using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Quacks : MonoBehaviour
{
    public AudioSource quackSource;

    public bool quacking = false;
    private bool canQuack = true;
    
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (quackSource && canQuack)
            {
                quackSource.Play();
                
                quacking = true;
                StartCoroutine(nameof(EndQuacking));
                
                canQuack = false;
                StartCoroutine(nameof(QuackCooldown));
            }
        }
    }

    IEnumerator QuackCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        canQuack = true;
    }

    IEnumerator EndQuacking()
    {
        yield return new WaitForSeconds(0.2f);
        quacking = false;
    }
}