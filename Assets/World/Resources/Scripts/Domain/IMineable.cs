namespace Features.Resources.Domain
{
    public interface IMineable
    {
        /// <summary> Нанести майнинг-урон. Вернёт true, если объект разрушен. </summary>
        bool Mine(float amount);

        /// <summary> Прогресс добычи 0..1. </summary>
        float GetProgress();

        bool IsDepleted { get; }
    }
}
