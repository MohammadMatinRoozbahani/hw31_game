using UnityEngine;
using UnityEngine.InputSystem;

public class Ship : MonoBehaviour
{
    [SerializeField] Stage stage;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animation blink;
    [SerializeField] float speed = 6.4f;
    [SerializeField] float marginX = 0.46f;
    [SerializeField] float marginY = 0.52f;
    [SerializeField] float topMargin = 2.37f;
    [SerializeField] WeaponDataSO[] weapons;
    [SerializeField] Pooled star;

    Vector2 move;
    bool fire;
    bool tap;
    float nextShot;
    float safeUntil;

    public Vector2 Pos => rb.position;
    public WeaponDataSO Weapon => weapons[GameManager.I.Power - 1];

    public void Move(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    public void Fire(InputAction.CallbackContext ctx)
    {
        fire = ctx.ReadValueAsButton();
        if (ctx.performed && stage.CanShoot)
            tap = true;
    }

    public void Missile(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && stage.CanShoot && GameManager.I.UseMissile())
            stage.LaunchMissile(Pos);
    }

    public void Star(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !stage.CanShoot || !GameManager.I.UseStar())
            return;

        Vector2 pos = Pos + Vector2.up * 0.43f;
        for (int i = -3; i <= 3; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, i * 12) * Vector2.up;
            stage.Spawn(star, pos, dir * 15.5f, 3);
        }
    }

    public void Back(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            stage.Back();
    }

    public void Retry(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            stage.Retry();
    }

    void FixedUpdate()
    {
        if (!stage.Live)
            return;

        Vector2 pos = rb.position + Vector2.ClampMagnitude(move, 1) * (speed * Time.fixedDeltaTime);
        pos.x = Mathf.Clamp(pos.x, -Frame.Width + marginX, Frame.Width - marginX);
        pos.y = Mathf.Clamp(pos.y, -Frame.Height + marginY, Frame.Height - topMargin);
        rb.MovePosition(pos);
    }

    void Update()
    {
        if (!stage.CanShoot)
        {
            tap = false;
            return;
        }

        if ((!fire && !tap) || Time.time < nextShot)
            return;

        WeaponDataSO gun = Weapon;
        nextShot = Time.time + gun.delay;
        tap = false;
        for (int i = 0; i < gun.barrels.Length; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, gun.angles[i]) * Vector2.up;
            stage.Spawn(gun.bullet, Pos + gun.barrels[i], dir * gun.speed, gun.damage);
        }
    }

    public void Hit()
    {
        if (!stage.Live || Time.time < safeUntil)
            return;

        safeUntil = Time.time + 1.44f;
        GameManager.I.Hit();

        if (GameManager.I.Lives <= 0)
        {
            rb.simulated = false;
            blink.gameObject.SetActive(false);
            stage.Lose();
            return;
        }

        blink.Stop();
        blink.Play();
    }
}
