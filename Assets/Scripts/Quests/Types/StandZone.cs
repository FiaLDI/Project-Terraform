using UnityEngine;
using Quests; 

namespace Quests
{
    public class StandZone : MonoBehaviour
    {
        private StandOnPointQuest quest;
        private bool playerInside = false;

        public void Initialize(StandOnPointQuest linkedQuest)
        {
            quest = linkedQuest;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = true;
                Debug.Log("Игрок вошел в зону стояния");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = false;
                Debug.Log("Игрок вышел из зоны стояния");
            }
        }

        private void Update()
        {
            if (playerInside && quest != null && quest.isActive)
            {
                quest.UpdateStandTime(Time.deltaTime);
            }
        }

        void OnDrawGizmos()
        {
            if (quest != null)
            {
                Gizmos.color = playerInside ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
            }
        }
    }
}