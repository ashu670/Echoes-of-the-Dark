using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParanormalManager : MonoBehaviour
{
    [System.Serializable]
    public class ParanormalEvent
    {
        public EventType type;

        public float minSanity;
        public float maxSanity;
        public float triggerChance;

        public float sanityDrainBoost = 2f;
        public float duration = 3f;

        public AudioClip sound;
    }

    public List<ParanormalEvent> events;

    public float minDelay = 5f;
    public float maxDelay = 10f;

    float timer;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;
    }

    public void TryTrigger(PlayerSystem player)
    {
        if (timer > 0f || player.isDead) return;
        Debug.Log("Trying to trigger paranormal event. Player sanity: " + player.sanity);

        foreach (var e in events)
        {
            if (player.sanity <= e.maxSanity && player.sanity >= e.minSanity)
            {
                if (Random.value < e.triggerChance)
                {
                    TriggerEvent(e, player);
                    Debug.Log("Triggered event: " + e.type);
                    break;
                }
            }
        }

        ResetTimer();
    }

    void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }

    void TriggerEvent(ParanormalEvent e, PlayerSystem player)
    {
        if (e.type == EventType.Footstep)
        {
            StartCoroutine(Footstep(e, player));

            // sanity effect
            player.ApplySanityEffect(e.sanityDrainBoost, e.duration);
        }
    }

    IEnumerator Footstep(ParanormalEvent e, PlayerSystem player)
    {
        // small delay (important for horror timing)
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        Vector3 dir = -player.transform.forward;

        float side = Random.Range(-1f, 1f);
        dir += player.transform.right * side;

        float dist = Random.Range(1.5f, 3f);

        Vector3 pos = player.transform.position + dir.normalized * dist;

        GameObject obj = new GameObject("FootstepSound");
        obj.transform.position = pos;

        AudioSource source = obj.AddComponent<AudioSource>();

        source.clip = e.sound;
        source.spatialBlend = 1f;
        source.volume = Random.Range(0.8f, 1f);

        source.minDistance = 2f;
        source.maxDistance = 10f;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();

        Destroy(obj, e.sound.length);
    }
}