using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ParanormalEvent
{
    public EventType type;
    public float minSanity;
    public float maxSanity;
    public float triggerChance;

    public void Trigger(PlayerSystem player)
    {
        switch (type)
        {
            case EventType.Footstep:
                // play footstep
                PlayFootstep(player);
                break;

            case EventType.Appearance:
                // spawn fake enemy
                break;
        }
    }

    void PlayFootstep(PlayerSystem player)
    {
        Vector3 dir;

        // randomize direction (behind / left / right)
        float rand = Random.value;

        if (rand < 0.5f)
            dir = -player.transform.forward; // behind
        else if (rand < 0.75f)
            dir = player.transform.right; // right
        else
            dir = -player.transform.right; // left

        float distance = Random.Range(1.5f, 3f);

        Vector3 pos = player.transform.position + dir * distance;

        //AudioSource.PlayClipAtPoint(footstepClip, pos);
    }
}