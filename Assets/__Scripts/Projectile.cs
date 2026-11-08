using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Set Dynamically")]
    public Rigidbody rigid;
    public float speed = 20f; // Added so derived classes can use it

    [SerializeField]
    private WeaponType _type;

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    // Added a pos property just like in Enemy.cs
    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move(); // Call the virtual Move method

        // Check bounds based on who is shooting
        if (bndCheck != null)
        {
            if (gameObject.tag == "ProjectileHero" && bndCheck.offUp)
                Destroy(gameObject);
            else if (gameObject.tag == "ProjectileEnemy" && bndCheck.offDown)
                Destroy(gameObject);
            else if (bndCheck.offLeft || bndCheck.offRight)
                Destroy(gameObject);
        }
    }

    // Virtual method that base projectiles don't need to use (physics handles it)
    public virtual void Move() { }

    public void SetType(WeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GetWeaponDefinition(_type);
        if (rend != null && def == null && gameObject.tag == "ProjectileHero") 
        {
            rend.material.color = def.projectileColor;
        }
    }
}