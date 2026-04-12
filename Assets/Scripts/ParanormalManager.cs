using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParanormalManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_3 = new WaitForSeconds(0.3f);
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);

    [System.Serializable]
    public class ParanormalEvent
    {
        public EventType type;

        public float minSanity;
        public float maxSanity;
        public float triggerChance;

        public float sanityDrainBoost = 2f;
        public float duration = 3f;

        public AudioClip[] sounds;
        public GameObject ghostPrefab = null;
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
        switch (e.type)
        {
            case EventType.Footstep:
                StartCoroutine(Footstep(e, player));
                // sanity effect
                player.ApplySanityEffect(e.sanityDrainBoost, e.duration);
                break;

            case EventType.Whisper:
                StartCoroutine(Whisper(e, player));
                player.ApplySanityEffect(e.sanityDrainBoost, e.duration);
                break;

            case EventType.Appearance:
                StartCoroutine(Appearance(e, player));
                player.ApplySanityEffect(e.sanityDrainBoost, e.duration);
                break;
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

        GameObject obj = new("FootstepSound");
        obj.transform.position = pos;

        AudioSource source = obj.AddComponent<AudioSource>();

        source.clip = e.sounds[0];
        source.spatialBlend = 1f;
        source.volume = Random.Range(0.8f, 1f);

        source.minDistance = 2f;
        source.maxDistance = 10f;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();

        Destroy(obj, e.sounds[0].length);
    }

    IEnumerator Whisper(ParanormalEvent e, PlayerSystem player)
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        if (e.sounds == null || e.sounds.Length == 0)
            yield break;

        // pick random whisper
        AudioClip clip = e.sounds[Random.Range(0, e.sounds.Length)];

        // slight left/right offset (ear feeling)
        float side = Random.Range(-1f, 1f);
        Vector3 offset = 0.5f * side * player.transform.right;

        Vector3 pos = player.transform.position + offset;

        GameObject obj = new("WhisperSound");
        obj.transform.position = pos;

        AudioSource source = obj.AddComponent<AudioSource>();

        source.clip = clip;
        source.spatialBlend = 1f;

        // key for whisper feel
        source.volume = Random.Range(0.2f, 0.4f);
        source.pitch = Random.Range(0.9f, 1.1f);

        source.minDistance = 0.5f;
        source.maxDistance = 3f;

        source.Play();

        Destroy(obj, clip.length);
    }

    IEnumerator Appearance(ParanormalEvent e, PlayerSystem player)
    {
        if (e.ghostPrefab == null)
            yield break;

        Vector3 spawnDir;

        // anticipate turn → spawn ahead of where player is turning
        if (player.turnEventActive)
        {
            float anticipation = player.turnDirection * 35f;
            spawnDir = Quaternion.Euler(0f, anticipation, 0f) * player.transform.forward;
        }
        else if (player.isEnteringRoom)
        {
            spawnDir = player.transform.forward;
        }
        else
        {
            spawnDir = player.transform.forward;
        }

        // slight randomness to avoid perfect placement
        spawnDir = Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f) * spawnDir;

        // FAR distance (important change)
        float distance = Random.Range(7f, 9f);

        Vector3 spawnPos = player.transform.position + spawnDir.normalized * distance;

        GameObject ghost = Instantiate(e.ghostPrefab, spawnPos, Quaternion.identity);

        // imperfect facing (feels natural)
        Vector3 dir = (player.transform.position - ghost.transform.position).normalized;
        dir += Random.insideUnitSphere * 0.05f;
        ghost.transform.forward = dir;

        // HOLD (player processes presence)
        float holdTime = Random.Range(0.5f, 0.8f);
        float t = 0f;

        while (t < holdTime)
        {
            t += Time.deltaTime;

            // slight instability
            ghost.transform.position += Random.insideUnitSphere * 0.01f;

            yield return null;
        }

        // acceleration charge (not linear)
        float speed = 0f;
        float accel = 25f;

        while (ghost != null)
        {
            speed += accel * Time.deltaTime;

            ghost.transform.position = Vector3.MoveTowards(
                ghost.transform.position,
                player.transform.position,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(ghost.transform.position, player.transform.position) < 1.8f)
                break;

            yield return null;
        }

        Destroy(ghost);
    }
}