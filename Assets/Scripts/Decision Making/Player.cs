using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerWeaponType : int
{
    NONE,
    SHOTGUN,
    SNIPER
}

public class Player : MonoBehaviour
{
    [SerializeField]
    GameObject bulletPrefab;
    [SerializeField]
    GameObject shotgunPrefab;
    [SerializeField]
    GameObject sniperPrefab;

    Health health;

    Rigidbody2D rb;
    const float moveForce = 50.0f;
    const float maxSpeed = 10.0f;
    Vector2 direction = Vector2.zero;

    PlayerWeaponType weaponType = PlayerWeaponType.NONE;

    Timer shootCooldown = new Timer();

    const float cooldownSniper = 0.75f;
    const float cooldownShotgun = 0.25f;
    const float cooldownDefault = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        SpawnWeapons();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector2.up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector2.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction += Vector2.right;
        }
        direction = direction.normalized;

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;
        Vector3 mouseDirection = (mouse - transform.position).normalized;

        shootCooldown.Tick(Time.deltaTime);

        if (Input.GetMouseButtonDown(0) && shootCooldown.Expired())
        {
            switch (weaponType)
            {
                case PlayerWeaponType.SHOTGUN:
                    ShootShotgun(mouseDirection);
                    shootCooldown.Reset(cooldownShotgun);
                    break;

                case PlayerWeaponType.SNIPER:
                    ShootSniper(mouseDirection);
                    shootCooldown.Reset(cooldownSniper);
                    break;

                case PlayerWeaponType.NONE:
                default:
                    Utilities.CreateBullet(bulletPrefab, transform.position, mouseDirection, 15.0f, 25.0f, UnitType.PLAYER);
                    shootCooldown.Reset(cooldownDefault);
                    break;
            }
        }

        // Respawn player if health is below zero
        if (health.health <= 0.0f)
            Respawn();
    }

    void FixedUpdate()
    {
        // Apply force based on input direction and reset for next input
        rb.AddForce(direction * moveForce);
        direction = Vector2.zero;

        // Limit velocity
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    void Respawn()
    {
        health.health = Health.maxHealth;
        transform.position = new Vector3(0.0f, -3.0f);
        weaponType = PlayerWeaponType.NONE;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Shotgun"))
        {
            weaponType = PlayerWeaponType.SHOTGUN;
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Sniper"))
        {
            weaponType = PlayerWeaponType.SNIPER;
            Destroy(collision.gameObject);
        }
    }

    void SpawnWeapons()
    {

        Vector3 shotgunPosition = new Vector3(Random.Range(-8.3f, 8.3f), Random.Range(-4.5f, 4.5f), 0);
        Instantiate(shotgunPrefab, shotgunPosition, Quaternion.identity);

        Vector3 sniperPosition = new Vector3(Random.Range(-8.3f, 8.3f), Random.Range(-4.5f, 4.5f), 0);
        Instantiate(sniperPrefab, sniperPosition, Quaternion.identity);
    }

    void ShootShotgun(Vector3 direction)
    {
        Vector3 left = Quaternion.Euler(0.0f, 0.0f, 30.0f) * direction;
        Vector3 right = Quaternion.Euler(0.0f, 0.0f, -30.0f) * direction;

        Utilities.CreateBullet(bulletPrefab, transform.position, direction, 10.0f, 20.0f, UnitType.PLAYER);
        Utilities.CreateBullet(bulletPrefab, transform.position, left, 10.0f, 20.0f, UnitType.PLAYER);
        Utilities.CreateBullet(bulletPrefab, transform.position, right, 10.0f, 20.0f, UnitType.PLAYER);
    }

    void ShootSniper(Vector3 direction)
    {
        Utilities.CreateBullet(bulletPrefab, transform.position, direction, 20.0f, 50.0f, UnitType.PLAYER);
    }
}
