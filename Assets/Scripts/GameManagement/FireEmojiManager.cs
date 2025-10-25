using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireEmojiManager : MonoBehaviour
{
    [Header("References")]
    public RectTransform canvasRect;
    public GameObject fireEmojiPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 0.1f;
    public int maxFireEmojis = 50;

    private bool isActive = false;
    private Coroutine spawnRoutine;
    private List<GameObject> activeEmojis = new List<GameObject>();

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        // Remove all spawned fires
        foreach (var fire in activeEmojis)
            if (fire != null) Destroy(fire);

        activeEmojis.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            if (activeEmojis.Count < maxFireEmojis)
            {
                SpawnFireEmoji();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnFireEmoji()
    {
        GameObject fire = Instantiate(fireEmojiPrefab, canvasRect);
        RectTransform fireRect = fire.GetComponent<RectTransform>();

        fireRect.anchoredPosition = new Vector2(
            Random.Range(-canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2),
            Random.Range(-canvasRect.sizeDelta.y / 2, canvasRect.sizeDelta.y / 2)
        );

        float randomScale = Random.Range(0.5f, 2.5f);
        fireRect.localScale = new Vector3(randomScale, randomScale, 1f);

        FireEmojiMover mover = fire.GetComponent<FireEmojiMover>();
        if (mover == null)
        {
            Debug.LogError("Spawned fire prefab has no FireEmojiMover attached!");
        }
        else
        {
            mover.Initialize(canvasRect);
            Debug.Log("Spawned a new fire emoji at position " + fireRect.anchoredPosition);
        }

        activeEmojis.Add(fire);
    }

}
