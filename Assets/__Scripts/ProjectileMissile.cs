using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMissile : Projectile
{
    public float turnSpeed = 5f;
    private Transform target;

    void Start()
    {
        // Stop the rigidbody from moving it in a straight line
        rigid.velocity = Vector3.zero;

        // Find all objects in the scene that have the Enemy script
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float closestDist = Mathf.Infinity;
        
        foreach (Enemy enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                target = enemy.transform;
            }
        }
    }

    public override void Move()
    {
        if (target != null)
        {
            // Rotate towards the target
            Vector3 vectorToTarget = target.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * turnSpeed);
        }
        
        // Move forward in the direction we are pointing
        transform.position += transform.up * speed * Time.deltaTime;
    }
}