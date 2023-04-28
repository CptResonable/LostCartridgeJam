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

    public void FireProjectile(GameObject goProjectilePrefab, Gun gun, Character firedByCharacter, Transform tMuzzle, Vector3 velocity, float damage) {

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(goProjectilePrefab.transform, tMuzzle.position - tMuzzle.forward * 0.05f, tMuzzle.rotation).gameObject;
        Bullet bullet = goBullet.GetComponent<Bullet>();
        //bullet.Fire(firedByCharacter, tMuzzle.forward * muzzleVelocity, damage);

    }
}

public struct ProjectileParams {
    public ProjectileParams(Vector3 origin, Vector3 muzzleVelocity, float damage, uint characterId) {
        this.origin = origin;
        this.muzzleVelocity = muzzleVelocity;
        this.damage = damage;
        this.characterId = characterId;
    }

    public Vector3 origin;
    public Vector3 muzzleVelocity;
    public float damage;
    public uint characterId;
}
