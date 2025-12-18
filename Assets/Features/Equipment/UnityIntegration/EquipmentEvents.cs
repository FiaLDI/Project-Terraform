using Features.Equipment.Domain;
using System;

public static class EquipmentEvents
{
    public static Action<IUsable, IUsable, bool> OnHandsUpdated;
}
