using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    static public Hero S; //Singleton

    [Header("Set in Inspector")]
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public int missileAmmo = 5;
    public Weapon[] weapons;
    public float showDamageDuration = 0.5f; // Increased slightly so i-frames are noticeable


    private int replaceIndex = 0; // Tracks which slot to overwrite when full
    // _shieldLevel starts at 3 and drops by 1 per hit
    private float _shieldLevel = 3; 
    
    // Damage Flash Variables
    private Material[] materials;
    private Color[] originalColors;
    private bool showingDamage = false;
    private float damageDoneTime;

    private GameObject lastTriggerGo = null;
    private BoundsCheck bndCheck; 
    private GameObject shieldVisual; 
    private Renderer shieldRend;

    public delegate void WeaponFireDelegate();
    public WeaponFireDelegate fireDelegate;

    void Start()
    {
        if(S == null) S = this; 
        else Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        
        bndCheck = GetComponent<BoundsCheck>(); 
        if (bndCheck != null) bndCheck.keepOnScreen = false; 
        
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }

        Transform shieldT = transform.Find("Shield");
        if (shieldT != null)
        {
            shieldVisual = shieldT.gameObject;
            shieldRend = shieldVisual.GetComponent<Renderer>();
        }

        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
        
        shieldLevel = 3; // Force start at 3
    }

    void Update()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;

        if (bndCheck != null)
        {
            // Left/Right Wrap
            if (pos.x > bndCheck.camWidth + bndCheck.radius) pos.x = -bndCheck.camWidth - bndCheck.radius;
            else if (pos.x < -bndCheck.camWidth - bndCheck.radius) pos.x = bndCheck.camWidth + bndCheck.radius;

            // Top/Bottom Clamp
            if (pos.y > bndCheck.camHeight - bndCheck.radius) pos.y = bndCheck.camHeight - bndCheck.radius;
            else if (pos.y < -bndCheck.camHeight + bndCheck.radius) pos.y = -bndCheck.camHeight + bndCheck.radius;
        }
        
        transform.position = pos;
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, yAxis * rollMult, 0);

        if (showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if (Input.GetAxis("Jump") == 1 && fireDelegate != null)
        {
            fireDelegate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        
        if (go == lastTriggerGo) return;
        lastTriggerGo = go;

        if (go.tag == "Enemy")
        {
            TakeDamage(); 
            Destroy(go);   
        }
        else if (go.tag == "ProjectileEnemy" || other.tag == "ProjectileEnemy")
        {
            TakeDamage();
            Destroy(other.gameObject);
        }
        else if(go.tag == "PowerUp")
        {
            AbsorbPowerUp(go);
        }
    }

    void TakeDamage()
    {
        // I-frames: If we are currently flashing red, ignore the hit
        if (showingDamage) return; 

        ShowDamage(); // Flash Red

        if (_shieldLevel > 0)
        {
            shieldLevel--; // Lose 1 shield point
        }
        else
        {
            // No shield left, ship is destroyed instantly
            Destroy(this.gameObject);
            Main.S.DelayedRestart(gameRestartDelay);
        }
    }

    void ShowDamage()
    {
        foreach (Material m in materials) m.color = Color.red;
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    void UnShowDamage()
    {
        for (int i = 0; i < materials.Length; i++) materials[i].color = originalColors[i];
        showingDamage = false;
    }

    void UpdateShieldVisuals()
    {
        if (shieldVisual == null || shieldRend == null) return;

        if (_shieldLevel <= 0)
        {
            shieldVisual.SetActive(false);
        }
        else
        {
            shieldVisual.SetActive(true);
            if (_shieldLevel == 1) shieldRend.material.color = Color.red;       
            else if (_shieldLevel == 2) shieldRend.material.color = Color.yellow;    
            else if (_shieldLevel == 3) shieldRend.material.color = Color.green;     
            else if (_shieldLevel >= 4) shieldRend.material.color = Color.cyan;      
        }
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type)
        {
            case WeaponType.shield:
                shieldLevel++;
                break;

            default:
                Weapon w = GetEmptyWeaponSlot();
                
                if(w != null)
                {
                    // Empty slot found, put the new weapon there
                    w.SetType(pu.type);
                }
                else
                {
                    // All slots full! Overwrite the current replaceIndex slot
                    weapons[replaceIndex].SetType(pu.type);
                    
                    // Move to the next slot for the next pickup
                    replaceIndex++;
                    
                    // If we reach the end of the array, loop back to Slot 0
                    if (replaceIndex >= weapons.Length)
                    {
                        replaceIndex = 0;
                    }
                }
                break;
        }
        pu.AbsorbedBy(this.gameObject);
    }

    public float shieldLevel
    {
        get { return _shieldLevel; }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            if (_shieldLevel < 0) _shieldLevel = 0; // Prevent negative numbers
            
            UpdateShieldVisuals(); 
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i=0; i<weapons.Length; i++)
        {
            if(weapons[i].type == WeaponType.none) return weapons[i];
        }
        return null;
    }

    void ClearWeapons()
    {
        foreach(Weapon w in weapons) w.SetType(WeaponType.none);
    }
}