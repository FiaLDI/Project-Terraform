using System;

namespace Features.Abilities.Domain
{
    public interface IAbilityHandler
    {
        /// <summary>Тип AbilitySO, который обрабатывает этот хендлер.</summary>
        Type AbilityType { get; }

        /// <summary>Выполнить способность.</summary>
        void Execute(AbilitySO ability, AbilityContext ctx);
    }
}
