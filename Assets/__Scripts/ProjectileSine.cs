using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSine : Projectile
{
    public float frequency = 5f;
    public float magnitude = 2f;
    
    // ADDED: 1 for normal wave, -1 for inverted wave
    [HideInInspector] public float waveMultiplier = 1f; 

    private float birthTime;

    void Start()
    {
        birthTime = Time.time;
        rigid.velocity = Vector3.zero; 
    }

    public override void Move()
    {
        Vector3 tempPos = pos;
        
        // Move forward
        tempPos += transform.up * speed * Time.deltaTime;
        
        // ADDED: Multiply by waveMultiplier (-1 flips the sine wave)
        tempPos += transform.right * Mathf.Sin((Time.time - birthTime) * frequency) * magnitude * waveMultiplier * Time.deltaTime * 60f;
        
        pos = tempPos;
    }
}