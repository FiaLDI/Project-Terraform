using UnityEngine;
using Features.Weapons.Domain;
using Features.Combat.Application;
using Features.Items.Domain;

namespace Features.Weapons.Application
{
    public class ProjectileService
    {
        private readonly CombatService combat;

        public ProjectileService(CombatService combat)
        {
            this.combat = combat;
        }

        public GameObject SpawnProjectile(
            ProjectileConfig config,
            Vector3 position,
            Vector3 direction)
        {
            GameObject obj = Object.Instantiate(config.projectilePrefab, position, Quaternion.LookRotation(direction));

            var proj = obj.GetComponent<IProjectile>();
            proj.Setup(config, combat);

            return obj;
        }
    }
}
