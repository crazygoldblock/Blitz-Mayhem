using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// pohyb hráče
/// používá se na všech hráčích v lokální hře
/// a jenom na jednom hráči ve hře po síti
/// </summary>
public class Controler: MonoBehaviour
{
    public static Controler instance;
    public static bool globalControl = true;

    public float lastDirection = 1f;
    public bool control = true;

    private int id;
    private float lastShot;
    private bool grounded = false;
    
    private Coroutine upgradeCoroutine;

    public Rigidbody2D rb;
    public PlayerKeyBinds binds;
    public SpriteRenderer spriteRenderer;
    
    private float bulletForce = Constants.BULLET_PUSH;
    private float jumpForce = Constants.PLAYER_JUMP;
    private float movementSpeed = Constants.PLAYER_SPEED;    
    private float fireRate = Constants.FIRE_RATE;

    private void Start()
    {
        globalControl = true;

        instance = this;

        rb = GetComponent<Rigidbody2D>();
        lastShot = Time.time;
    }
    private void Update()
    {
        // vypnout pohyb na začátku kola
        if (transform.position.y > 15)
            return;

        // vypnout pohyb na začátku kola a když je otevřený chat
        if (!control)
            return;

        if (!globalControl)
            return;

        bool leftPress = Input.GetKey(binds.left);
        bool rightPress = Input.GetKey(binds.right);
        bool jumpPress = Input.GetKey(binds.jump);
        bool actionPress = Input.GetKeyDown(binds.action);

        float horizontal = (leftPress ? 0f : 1f) + (rightPress ? 0f : -1f);
        float xVel = rb.velocity.x;

        // horizontální pohyb
        if (Mathf.Abs(rb.velocity.x) < movementSpeed / 10 || Mathf.Sign(xVel) != horizontal)
            rb.velocity += new Vector2(horizontal * movementSpeed * Time.deltaTime, 0);
        
        // skok
        if (jumpPress && grounded)
        {
            rb.AddForce(new Vector2(0, jumpForce));
            grounded = false;
        }

        if (horizontal != 0f)
            lastDirection = horizontal;
        
        // výstřel
        if (actionPress && (Time.time - lastShot > fireRate))
        {
            GeneralGameManager.generalInstance.ShootBullet(transform.position, lastDirection, bulletForce);
            lastShot = Time.time;
        }

        if (rb.velocity.y > 10)
            rb.velocity = new Vector2(rb.velocity.x, 10);

        spriteRenderer.flipX = lastDirection == 1f;

        // smrt hráče pokud spadnul pryč z arény
        if (transform.position.y < -4)
        {
            gameObject.SetActive(false);
            GeneralGameManager.generalInstance.PlayerDeath(id);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherCollider.name != "PlayerHitbox")
            grounded = true;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
    }
    public void SetValues(PlayerKeyBinds binds, int id)
    {
        this.binds = binds;
        this.id = id;
    }
    public void BulletPush(float direction, float force)
    {
        // odstrčení hráče při zásahu

        float yVel = rb.velocity.y;
        float mul = 1;

        rb.velocity = Vector2.zero;

        if (!grounded)
            mul = 2;
        
        rb.AddForce(new Vector2(force * direction * mul, force));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Upgrade")
        {
            switch (collision.gameObject.GetComponent<Upgrade>().id)
            {
                case 0:
                    upgradeCoroutine = StartCoroutine(MovementSpeedUp());
                    break;
                case 1:
                    upgradeCoroutine = StartCoroutine(JumpForceUp());
                    break;
                case 2:
                    upgradeCoroutine = StartCoroutine(BulletForceUp());
                    break;
                case 3:
                    upgradeCoroutine = StartCoroutine(FireRateUp());
                    break;
            }
            GeneralGameManager.generalInstance.PickupUpgrade();
        }
    }
    IEnumerator MovementSpeedUp()
    {   
        movementSpeed = Constants.PLAYER_SPEED_UP;
        yield return new WaitForSeconds(Constants.UPGRADE_DURATION);
        movementSpeed = Constants.PLAYER_SPEED;
    }
    IEnumerator JumpForceUp()
    {   
        jumpForce = Constants.PLAYER_JUMP_UP;
        yield return new WaitForSeconds(Constants.UPGRADE_DURATION);
        jumpForce = Constants.PLAYER_JUMP;
    }
    IEnumerator BulletForceUp()
    {   
        bulletForce = Constants.BULLET_PUSH_UP;
        yield return new WaitForSeconds(Constants.UPGRADE_DURATION);
        bulletForce = Constants.BULLET_PUSH;
    }
    IEnumerator FireRateUp()
    {   
        fireRate = Constants.FIRE_RATE_UP;
        yield return new WaitForSeconds(Constants.UPGRADE_DURATION);
        fireRate = Constants.FIRE_RATE;
    }
    public void DisableAllUpgrades()
    {
        if (upgradeCoroutine != null) 
            StopCoroutine(upgradeCoroutine);
    }
    public void EnableControls(bool enabled)
    {
        control = enabled;
    }
}
[Serializable]
public class PlayerKeyBinds
{
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;
    public KeyCode action;

    public PlayerKeyBinds(KeyCode l, KeyCode r, KeyCode j, KeyCode a)
    {
        left = l;
        right = r;
        jump = j;
        action = a;
    }
} 
