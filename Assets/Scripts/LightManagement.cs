using UnityEditor;
using UnityEngine;

public class LightManagement : MonoBehaviour
{
    public float litRange = 5f;
    public LayerMask obstacleMask;
    bool isLit = false;

    PlayerSystem player;

    void Start()
    {
        var playerGo = GameObject.FindWithTag("Player");
        if (playerGo == null)
        {
            Debug.LogWarning("Player not found with tag \"Player\".");
            return;
        }
        player = playerGo.GetComponent<PlayerSystem>();
    }

    private void Update()
    {
        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, litRange);

        bool playerInside = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // 🔥 RAYCAST CHECK
                Vector3 dir = player.transform.position - transform.position;
                float dist = dir.magnitude;
                dir.Normalize();

                if (!Physics.Raycast(transform.position, dir, dist, obstacleMask))
                {
                    playerInside = true;
                }

                break;
            }
        }

        // ENTER
        if (playerInside && !isLit)
        {
            isLit = true;
            player.lightHit++;
        }
        // EXIT
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
