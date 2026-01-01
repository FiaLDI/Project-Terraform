using System;

namespace Features.Abilities.Domain
{
    public interface IAbilityHandler
    {
        /// <summary>
        /// Тип AbilitySO, который обрабатывает хендлер.
        /// </summary>
        Type AbilityType { get; }

        /// <summary>
        /// Серверное выполнение способности.
        /// Гарантированно вызывается ТОЛЬКО на сервере.
        /// </summary>
        void ExecuteServer(AbilitySO ability, AbilityContext ctx);
    }
}
