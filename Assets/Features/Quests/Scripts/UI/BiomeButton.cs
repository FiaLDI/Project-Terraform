using UnityEngine;


namespace Quests
{
    public class BiomeButton : MonoBehaviour
    {
        public BiomeQuestGiver giver;

        public void OnClick()
        {
            giver.GiveBiomeQuests();
        }
    }
    }
