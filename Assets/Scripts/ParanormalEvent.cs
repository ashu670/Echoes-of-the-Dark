using UnityEngine;

[System.Serializable]
public class ParanormalEvent
{
    public enum EventType
    {
        Footstep,
        Appearance
    }

    public EventType type;
    [Range(0f, 100f)] public float minSanity = 0f;
    [Range(0f, 100f)] public float maxSanity = 100f;
    [Range(0f, 1f)] public float triggerChance = 0.5f;

    // Data for specific events
    public AudioClip footstepClip;

    // Execute the event logic. PlayerSystem is expected to be a MonoBehaviour with a transform and a `sanity` float.
    public void Trigger(PlayerSystem player)
    {
        if (player == null) return;

        switch (type)
        {
            case EventType.Footstep:
                PlayFootstep(player);
                break;

            case EventType.Appearance:
                // Placeholder for appearance logic (spawn fake enemy, flash, etc.)
                // This should be implemented by adding relevant data fields to this class
                // or by using a separate handler system that listens for `ParanormalEvent`.
                break;
        }
    }

    void PlayFootstep(PlayerSystem player)
    {
        if (footstepClip == null) return;

        // Pick base direction: behind (50%), right (25%), left (25%)
        Vector3 dir;
        float r = Random.value;
        if (r < 0.5f)
            dir = -player.transform.forward; // behind
        else if (r < 0.75f)
            dir = player.transform.right; // right
        else
            dir = -player.transform.right; // left

        // Add slight angular randomness so sounds aren't perfectly aligned
        float angleOffset = Random.Range(-22.5f, 22.5f);
        dir = Quaternion.Euler(0f, angleOffset, 0f) * dir;
        dir.Normalize();

        float distance = Random.Range(1.5f, 3f);
        Vector3 pos = player.transform.position + dir * distance;

        // slight vertical offset so the sound isn't exactly on ground level
        pos += Vector3.up * Random.Range(0.1f, 0.5f);

        float volume = Random.Range(0.6f, 1f);
        AudioSource.PlayClipAtPoint(footstepClip, pos, volume);
    }
}