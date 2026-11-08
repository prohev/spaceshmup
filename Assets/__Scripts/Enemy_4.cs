
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Part
{
    // These three fields need to be defined in the Inspector
    public string name;
    public float health;
    public string[] protectedBy;

    // These fields are automatically assigned in Start()
    [HideInInspector]
    public GameObject go;

    [HideInInspector]
    public Renderer[] renderers;
}

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts;

    private Vector3 p0, p1;
    private float timeStart;
    private float duration = 4;

    void Start()
    {
        // Initialize movement
        p0 = p1 = pos;

        InitMovement();

        // Cache GameObjects and Renderers for each part
        foreach (Part prt in parts)
        {
            Transform t = transform.Find(prt.name);

            if (t != null)
            {
                prt.go = t.gameObject;

                // Find all Renderers on this part and its children
                prt.renderers = prt.go.GetComponentsInChildren<Renderer>();

                if (prt.renderers.Length == 0)
                {
                    Debug.LogError(
                        "Enemy_4: No Renderers found for part: " + prt.name,
                        prt.go
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "Enemy_4: Could not find part: " + prt.name,
                    this
                );
            }
        }
    }

    void InitMovement()
    {
        p0 = p1;

        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;

        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2);

        pos = (1 - u) * p0 + u * p1;
    }

    // Find a part by its name
    Part FindPart(string n)
    {
        foreach (Part prt in parts)
        {
            if (prt.name == n)
            {
                return prt;
            }
        }

        return null;
    }

    // Find a part by its GameObject
    Part FindPart(GameObject go)
    {
        foreach (Part prt in parts)
        {
            if (prt.go == go)
            {
                return prt;
            }
        }

        return null;
    }

    // Find the part associated with a collider,
    // including colliders on nested child objects
    Part FindPartFromCollider(Collider col)
    {
        if (col == null)
        {
            return null;
        }

        Transform t = col.transform;

        while (t != null)
        {
            Part prt = FindPart(t.gameObject);

            if (prt != null)
            {
                return prt;
            }

            t = t.parent;
        }

        return null;
    }

    // Check whether a part has been destroyed
    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }

    bool Destroyed(Part prt)
    {
        if (prt == null)
        {
            return true;
        }

        return prt.health <= 0;
    }

    // Change all Renderers belonging to one part to red
    void ShowLocalizedDamage(Part prt)
    {
        if (prt.renderers != null)
        {
            foreach (Renderer rend in prt.renderers)
            {
                if (rend != null)
                {
                    rend.material.color = Color.red;
                }
            }
        }

        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    // Handle projectile collisions
    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;

        switch (other.tag)
        {
            case "ProjectileHero":

                Projectile p = other.GetComponent<Projectile>();

                if (p == null)
                {
                    Debug.LogError("Enemy_4: Projectile component missing!", this);
                    Destroy(other);
                    return;
                }

                // Ignore damage while off screen
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }

                // Identify which part was hit
                Part prtHit = null;

                foreach (ContactPoint contact in coll.contacts)
                {
                    prtHit = FindPartFromCollider(contact.thisCollider);

                    if (prtHit == null)
                    {
                        prtHit = FindPartFromCollider(contact.otherCollider);
                    }

                    if (prtHit != null)
                    {
                        break;
                    }
                }

                // Stop if no valid part was identified
                if (prtHit == null)
                {
                    Debug.LogWarning(
                        "Enemy_4: Could not identify the part hit by projectile!",
                        this
                    );

                    Destroy(other);
                    return;
                }

                // Check whether this part is protected
                if (prtHit.protectedBy != null)
                {
                    foreach (string s in prtHit.protectedBy)
                    {
                        if (!Destroyed(s))
                        {
                            Destroy(other);
                            return;
                        }
                    }
                }

                // Apply projectile damage
                WeaponDefinition weaponDef =
                    Main.GetWeaponDefinition(p.type);

                if (weaponDef == null)
                {
                    Debug.LogError("Enemy_4: Weapon definition not found!", this);
                    Destroy(other);
                    return;
                }

                prtHit.health -= weaponDef.damageOnHit;

                // Show damage on the affected part
                ShowLocalizedDamage(prtHit);

                // Disable the part when its health reaches zero
                if (prtHit.health <= 0)
                {
                    prtHit.go.SetActive(false);
                }

                // Check whether every part has been destroyed
                bool allDestroyed = true;

                foreach (Part prt in parts)
                {
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }

                // Destroy the entire enemy when all parts are gone
                if (allDestroyed)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(gameObject);
                }

                // Destroy the projectile
                Destroy(other);

                break;
        }
    }
}
