using UnityEditor;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public float litRange = 5f;
    public LayerMask obstacleMask; // assign walls layer in inspector

    PlayerSystem player;
    bool isLit = false;

    void Start()
    {
        GameObject playerGo = GameObject.FindWithTag("Player");

        if (playerGo != null)
        {
            player = playerGo.GetComponent<PlayerSystem>();
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }

    void Update()
    {
        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, litRange);

        bool playerInside = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // RAYCAST CHECK
                Vector3 dir = player.transform.position - transform.position;
                float dist = dir.magnitude;
                dir.Normalize();

                // if ray hits obstacle → blocked
                if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
                {
                    playerInside = true;
                }

                break;
            }
        }

        // ENTER LIGHT
        if (playerInside && !isLit)
        {
            isLit = true;
            player.lightHit++;
        }
        // EXIT LIGHT
        else if (!playerInside && isLit)
        {
            isLit = false;
            player.lightHit--;
        }
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, litRange);
    }
}