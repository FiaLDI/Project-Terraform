namespace Features.Weapons.Domain
{
    /// <summary>
    /// Runtime состояние боеприпасов у ItemInstance.
    /// Хранится в ItemInstance, НЕ в ScriptableObject.
    /// </summary>
    public class WeaponAmmoState
    {
        public int ammoInMagazine;
        public bool isReloading;

        public WeaponAmmoState(int initial)
        {
            ammoInMagazine = initial;
        }
    }
}
