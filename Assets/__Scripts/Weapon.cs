using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    none,
    blaster,
    spread,
    phaser,
    missile,
    laser,
    shield
}

[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter;
    public Color color = Color.white;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float continuousDamage = 0;
    public float delayBetweenShots = 0;
    public float velocity = 20;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime;
    private Renderer collarRend;

    void Awake()
    {
        if (collar == null)
        {
            Transform collarT = transform.Find("Collar");
            if (collarT != null) collar = collarT.gameObject;
        }

        if (collar != null) collarRend = collar.GetComponent<Renderer>();
        if (collarRend == null) collarRend = GetComponent<Renderer>();
    }

    void Start()
    {
        SetType(_type);

        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        
        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Hero>() != null)
        {
            rootGO.GetComponent<Hero>().fireDelegate += Fire;
        }
    }

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        def = Main.GetWeaponDefinition(_type);

        if (collarRend != null && def != null)
        {
            collarRend.material.color = def.color;
        }
        
        lastShotTime = 0; 
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy) return;
        
        if (def == null || def.projectilePrefab == null)
        {
            Debug.LogError("No projectile prefab assigned in Main for weapon type: " + type);
            return;
        }

        if (Time.time - lastShotTime < def.delayBetweenShots)
        {
            return;
        }

        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0) vel.y = -vel.y;

        switch (type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                if (p != null) p.rigid.velocity = vel;
                break;

            case WeaponType.spread:
                p = MakeProjectile();
                if (p != null) p.rigid.velocity = vel;
                
                p = MakeProjectile(); 
                if (p != null) {
                    p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                    p.rigid.velocity = p.transform.rotation * vel;
                }
                
                p = MakeProjectile(); 
                if (p != null) {
                    p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                    p.rigid.velocity = p.transform.rotation * vel;
                }
                break;

            case WeaponType.phaser:
                // 1. Shoot the normal wave
                p = MakeProjectile();
                if (p != null) 
                {
                    p.rigid.velocity = vel; 
                }
                
                // 2. Shoot the inverted (-sin) wave
                p = MakeProjectile();
                if (p != null) 
                {
                    p.rigid.velocity = vel;
                    
                    // Flip the sine wave on the second bullet
                    ProjectileSine sineScript = p.GetComponent<ProjectileSine>();
                    if (sineScript != null) 
                    {
                        sineScript.waveMultiplier = -1f;
                    }
                }
                break;

            // ADDED MISSILE CASE
            case WeaponType.missile:
                p = MakeProjectile();
                // We don't set rigid.velocity here because the Missile script handles its own movement
                break;
        }
    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.root.CompareTag("Hero"))
        {
            go.tag = "ProjectileHero";
            int layer = LayerMask.NameToLayer("ProjectileHero");
            if (layer != -1) go.layer = layer;
        }
        else
        {
            go.tag = "ProjectileEnemy";
            int layer = LayerMask.NameToLayer("ProjectileEnemy");
            if (layer != -1) go.layer = layer;
        }

        Vector3 spawnPos = (collar != null) ? collar.transform.position : transform.position;
        go.transform.position = spawnPos;
        
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        
        if (p != null)
        {
            p.type = type;
            p.speed = def.velocity; // <--- IMPORTANT: Syncs the Main inspector speed to the projectile!
        }
        lastShotTime = Time.time;
        return (p);
    }
}