using UnityEngine;

public class RitualBody : MonoBehaviour
{
    public string actionID;

    GameManager manager;

    [System.Obsolete]
    void Start()
    {
        manager = FindFirstObjectByType<GameManager>();
    }

    public void Interact()
    {
        manager.PerformRitual(actionID);
    }
}