using Features.Resources.Domain;

namespace Features.Resources.Application
{
    public class MiningService
    {
        /// <summary>
        /// Применяет майнинг-урон с учётом модификатора инструмента.
        /// </summary>
        public bool Mine(IResourceNode node, float baseAmount, float toolMultiplier)
        {
            if (node.IsDepleted) return true;

            float final = baseAmount * toolMultiplier;
            return node.Mine(final);
        }
    }
}
