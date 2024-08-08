using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType : int
{
    NONE,
    SHOTGUN,
    SNIPER
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    Transform player;

    Health health;
    Rigidbody2D rb;

    [SerializeField]
    Transform[] waypoints;
    int waypoint = 0;

    WeaponType weaponType = WeaponType.NONE;

    const float moveSpeed = 7.5f;
    const float turnSpeed = 1080.0f;
    const float viewDistance = 5.0f;

    [SerializeField]
    GameObject bulletPrefab;
    [SerializeField]
    GameObject shotgunPrefab;
    [SerializeField]
    GameObject sniperPrefab;

    Timer shootCooldown = new Timer();
    Timer weaponSwitchTimer = new Timer(5.0f); 

    const float cooldownSniper = 0.75f;
    const float cooldownShotgun = 0.25f;

    enum State
    {
        NEUTRAL,
        OFFENSIVE,
        DEFENSIVE
    };

    State statePrev, stateCurr;
    Color color;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        Respawn();
        SpawnWeapons();
    }

    void Update()
    {
        float rotation = Steering.RotateTowardsVelocity(rb, turnSpeed, Time.deltaTime);
        rb.MoveRotation(rotation);

        if (health.health <= 0.0f)
            Respawn();

        if (Input.GetKeyDown(KeyCode.T))
        {
            health.health *= 0.25f;
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            int type = (int)weaponType;
            type++;
            type = type % 3;
            weaponType = (WeaponType)type;
        }

        if (stateCurr != State.DEFENSIVE)
        {
            float playerDistance = Vector2.Distance(transform.position, player.position);
            stateCurr = playerDistance <= viewDistance ? State.OFFENSIVE : State.NEUTRAL;

            if (health.health <= Health.maxHealth * 0.25f)
                stateCurr = State.DEFENSIVE;
        }

        if (stateCurr != statePrev)
            OnTransition(stateCurr);

        switch (stateCurr)
        {
            case State.NEUTRAL:
                Patrol();
                break;

            case State.OFFENSIVE:
                Attack();
                break;

            case State.DEFENSIVE:
                Defend();
                break;
        }

        statePrev = stateCurr;
        Debug.DrawLine(transform.position, transform.position + transform.right * viewDistance, color);

        
        if (weaponType == WeaponType.SHOTGUN | weaponType == WeaponType.SNIPER)
        {
            weaponSwitchTimer.Tick(Time.deltaTime);
            if (weaponSwitchTimer.Expired())
            {
                weaponType = weaponType == WeaponType.SHOTGUN ? WeaponType.SNIPER : WeaponType.SHOTGUN;
                weaponSwitchTimer.Reset();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            
        }

        if (collision.CompareTag("Shotgun"))
        {
            weaponType = WeaponType.SHOTGUN;
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Sniper"))
        {
            weaponType = WeaponType.SNIPER;
            Destroy(collision.gameObject);
        }
    }

    void Attack()
    {
        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Seek(rb, player.position, moveSpeed);
        rb.AddForce(steeringForce);

        Shoot();
    }

    void Defend()
    {
        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Flee(rb, player.position, moveSpeed);
        rb.AddForce(steeringForce);

        Shoot();
    }

    void Patrol()
    {
        float distance = Vector2.Distance(transform.position, waypoints[waypoint].transform.position);
        if (distance <= 2.5f)
        {
            waypoint++;
            waypoint %= waypoints.Length;
        }

        Vector3 steeringForce = Vector2.zero;
        steeringForce += Steering.Seek(rb, waypoints[waypoint].transform.position, moveSpeed);
        rb.AddForce(steeringForce);
    }

    void Shoot()
    {
        Vector3 playerDirection = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, playerDirection, viewDistance);
        bool playerHit = hit && hit.collider.CompareTag("Player");

        float playerDistance = Vector2.Distance(transform.position, player.position);

        shootCooldown.Tick(Time.deltaTime);
        if (playerHit && shootCooldown.Expired())
        {
            shootCooldown.Reset();
            if (weaponType == WeaponType.SHOTGUN && playerDistance <= 300)
            {
                ShootShotgun();
                Debug.Log("Shotgun");
            }
            else if (weaponType == WeaponType.SNIPER && playerDistance > 300)
            {
                ShootSniper();
                Debug.Log("Sniper");
            }
        }
    }

    void ShootShotgun()
    {
        Vector3 forward = (player.position - transform.position).normalized;
        Vector3 left = Quaternion.Euler(0.0f, 0.0f, 30.0f) * forward;
        Vector3 right = Quaternion.Euler(0.0f, 0.0f, -30.0f) * forward;

        Utilities.CreateBullet(bulletPrefab, transform.position, forward, 10.0f, 20.0f, UnitType.ENEMY);
        Utilities.CreateBullet(bulletPrefab, transform.position, left, 10.0f, 20.0f, UnitType.ENEMY);
        Utilities.CreateBullet(bulletPrefab, transform.position, right, 10.0f, 20.0f, UnitType.ENEMY);
    }

    void ShootSniper()
    {
        Vector3 forward = (player.position - transform.position).normalized;
        Utilities.CreateBullet(bulletPrefab, transform.position, forward, 20.0f, 50.0f, UnitType.ENEMY/**Time.deltaTime*/);
    }

    void OnTransition(State state)
    {
        switch (state)
        {
            case State.NEUTRAL:
                color = Color.magenta;
                waypoint = Utilities.NearestPosition(transform.position, waypoints);
                break;

            case State.OFFENSIVE:
                color = Color.red;
                break;

            case State.DEFENSIVE:
                color = Color.blue;
                break;
        }
        GetComponent<SpriteRenderer>().color = color;
    }

    void Respawn()
    {
        statePrev = stateCurr = State.NEUTRAL;
        OnTransition(stateCurr);
        health.health = Health.maxHealth;
        transform.position = new Vector3(0.0f, 3.0f);
        weaponType = WeaponType.NONE;
    }

    void SpawnWeapons()
    {
        
        Vector3 shotgunPosition = new Vector3(Random.Range(-8.3f, 8.3f), Random.Range(-4.5f, 4.5f), 0);
        Instantiate(shotgunPrefab, shotgunPosition, Quaternion.identity);

        Vector3 sniperPosition = new Vector3(Random.Range(-8.3f, 8.3f), Random.Range(-4.5f, 4.5f), 0);
        Instantiate(sniperPrefab, sniperPosition, Quaternion.identity);
    }
}
