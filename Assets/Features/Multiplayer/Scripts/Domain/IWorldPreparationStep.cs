using System;

public interface IWorldPreparationStep
{
    void Prepare(Action onFinished);
}
