using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<RitualAction> ritualActions;
    public GameObject gameCompleteUI;
    int currentIndex = 0;

    public EnemyAI enemy;
    public ParanormalManager paranormal;
    public PlayerSystem player;

    bool isPerforming = false;
    Coroutine currentRoutine;

    public void PerformRitual(string actionID)
    {
        if (isPerforming) return;

        if (currentIndex >= ritualActions.Count) return;

        RitualAction expected = ritualActions[currentIndex];

        if (actionID == expected.actionID)
        {
            currentRoutine = StartCoroutine(ExecuteRitual(expected));
        }
        else
        {
            WrongRitual();
        }
    }

    IEnumerator ExecuteRitual(RitualAction action)
    {
        isPerforming = true;

        float t = 0f;
        Vector3 startPos = player.transform.position;

        while (t < action.duration)
        {
            t += Time.deltaTime;

            float move = Vector3.Distance(player.transform.position, startPos);

            // interrupt if player moves
            if (move > 0.2f)
            {
                isPerforming = false;
                yield break;
            }

            // increase noise during ritual
            player.noiseVal = 1f;

            yield return null;
        }

        CompleteRitual(action);
        isPerforming = false;
    }

    void CompleteRitual(RitualAction action)
    {
        action.completed = true;
        currentIndex++;

        // outcomes

        if (action.triggerParanormal && paranormal != null)
        {
            paranormal.ForceTrigger(action.forcedEvent, player);
        }

        if (action.triggerLightFlicker)
        {
            // hook later with LightManager
        }

        if (action.triggerHunt)
        {
            enemy.StartForcedHunt(10f);

            // force enemy to player location immediately
            enemy.GetComponent<UnityEngine.AI.NavMeshAgent>()
                 .SetDestination(player.transform.position);
        }

        if (player != null)
            player.sanity -= action.sanityImpact;

        // escalation
        enemy.huntTimer += 5f;
        enemy.huntSanity += 5f;

        if (currentIndex >= ritualActions.Count)
        {
            RitualCompleted();
        }
    }

    void WrongRitual()
    {
        player.sanity -= 10f;

        if (paranormal != null)
        {
            paranormal.ForceTrigger(EventType.Appearance, player);
        }

        enemy.StartForcedHunt(10f);
        enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(player.transform.position);
    }

    void RitualCompleted()
    {
        Debug.Log("All Rituals Done → Escape Enabled");

        if (gameCompleteUI != null) { }
            gameCompleteUI.SetActive(true);
        
        enemy.GameOver = true;
    }
}

[System.Serializable]
public class RitualAction
{
    public string actionID;

    public bool requiresItem;
    public string requiredItemID;

    public float duration;

    public bool triggerHunt;
    public bool triggerParanormal;
    public EventType forcedEvent;

    public bool triggerLightFlicker;
    public float sanityImpact;

    public bool completed;
}