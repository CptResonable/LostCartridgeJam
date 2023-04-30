using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour {
    private static ProjectileManager _i;
    public static ProjectileManager i {
        get {
            if (_i == null) {
                GameObject go = new GameObject("ProjectileManager");
                _i = go.AddComponent<ProjectileManager>();
            }

            return _i;
        }
    }

    public Dictionary<uint, Bullet> projectiles = new Dictionary<uint, Bullet>();

    public delegate void ProjectileHitCharacterDelegate(ProjectileHitCharacterParams projectileHitCharacterParams);
    public event ProjectileHitCharacterDelegate projectileHitCharacterEvent;

    public void FireProjectile(GameObject goProjectilePrefab, Gun gun, Character firedByCharacter, Transform tMuzzle, Vector3 velocity, float damage) {

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(goProjectilePrefab.transform, tMuzzle.position, tMuzzle.rotation).gameObject;

        ProjectileParams projectileParams = new ProjectileParams(tMuzzle.position, velocity, damage, IdManager.CreateNewID(), firedByCharacter.ID);
        Bullet bullet = goBullet.GetComponent<Bullet>();
        bullet.Fire(projectileParams);

    }

    public void ProjectileHit(Bullet projectile, RaycastHit hit, Vector3 projectilePath) {
        EffectMaterialKeeper materialKeeper;
        if (hit.transform.TryGetComponent<EffectMaterialKeeper>(out materialKeeper)) {

            materialKeeper.PlayBulletHitEffects(hit.point, hit.normal);
        }

        DamageReceiver damageReceiver;
        if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
            damageReceiver.ReceiveDamage_bulletHit(projectile.projectileParams, hit.point, projectilePath);
        }

        Debug.Log("Hit 1 " + hit.collider.gameObject.layer);
        // A character is hit
        if (LayerMasks.IsInLayerMask(hit.collider.gameObject.layer, LayerMasks.i.characters)) {
            Transform tCharacter;
            Debug.Log("Hit 2");
            if (Utils.TryFindParentByName(hit.collider.transform, "Character", out tCharacter)) {
                Debug.Log("Hit 3");
                Character character = tCharacter.GetComponent<Character>();
                ProjectileHitCharacterParams characterHitParams = new ProjectileHitCharacterParams(projectile.projectileParams, character.ID, hit);
                projectileHitCharacterEvent?.Invoke(characterHitParams);
            } 
        }
    }
}

public struct ProjectileParams {
    public ProjectileParams(Vector3 origin, Vector3 muzzleVelocity, float damage, uint projectileID, uint characterId) {
        this.origin = origin;
        this.muzzleVelocity = muzzleVelocity;
        this.damage = damage;
        this.projectileID = projectileID;
        this.characterId = characterId;
    }

    public uint projectileID;
    public uint characterId;
    public Vector3 origin;
    public Vector3 muzzleVelocity;
    public float damage;
}

public struct ProjectileHitCharacterParams {
    public ProjectileHitCharacterParams(ProjectileParams projectileParams, uint hitCharacterID, RaycastHit hit) {
        this.projectileParams = projectileParams;
        this.hitCharacterID = hitCharacterID;
        this.hit = hit;
    }

    public ProjectileParams projectileParams;
    public uint hitCharacterID;
    public RaycastHit hit;
}
