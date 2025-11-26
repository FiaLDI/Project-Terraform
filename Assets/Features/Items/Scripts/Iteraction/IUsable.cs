using UnityEngine;

public interface IUsable
{
    void Initialize(Camera cam);

    void OnUsePrimary_Start();
    void OnUsePrimary_Hold();
    void OnUsePrimary_Stop();

    void OnUseSecondary_Start();
    void OnUseSecondary_Hold();
    void OnUseSecondary_Stop();
}
