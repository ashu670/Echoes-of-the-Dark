using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParanormalManager : MonoBehaviour
{
    [Tooltip("List of possible paranormal events (non-MonoBehaviour data objects).")]
    public List<ParanormalEvent> events = new List<ParanormalEvent>();

    [Tooltip("Minimum cooldown (seconds) between attempts.")]
    public float minCooldown = 5f;
    [Tooltip("Maximum cooldown (seconds) between attempts.")]
    public float maxCooldown = 12f;

    float nextAttemptTime = 0f;

    void Start()
    {
        ResetCooldown();
    }

    void ResetCooldown()
    {
        nextAttemptTime = Time.time + Random.Range(minCooldown, maxCooldown);
    }

    // Called by EnemyAI (or other controllers). Returns true if an event was triggered.
    public bool TryTriggerEvent(PlayerSystem player)
    {
        if (player == null) return false;

        if (Time.time < nextAttemptTime) return false;

        // Filter events by player sanity
        var candidates = events.Where(e => player.sanity >= e.minSanity && player.sanity <= e.maxSanity).ToList();

        if (candidates.Count == 0)
        {
            ResetCooldown();
            return false;
        }

        // Optionally shuffle candidates so checks are not always in the same order
        for (int i = 0; i < candidates.Count; i++)
        {
            int j = Random.Range(i, candidates.Count);
            var tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        // Evaluate each candidate's chance and trigger the first one that wins
        foreach (var e in candidates)
        {
            if (Random.value < e.triggerChance)
            {
                e.Trigger(player);
                ResetCooldown();
                return true;
            }
        }

        // No event triggered this attempt; still reset cooldown to avoid spamming checks
        ResetCooldown();
        return false;
    }
}
