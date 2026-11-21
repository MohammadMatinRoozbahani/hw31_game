using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] int maxHp = 100;
    [SerializeField] int points = 2000;
    [SerializeField] float speed = 1.8f;
    [SerializeField] float eggSpeed = 4;
    [SerializeField] int eggCount = 3;
    [SerializeField] float volleyDelay = 1.6f;
    [SerializeField] float volleySpread = 12;
    [SerializeField] int summonCount = 2;
    [SerializeField] WaveDataSO helpers;

    Stage stage;
    int hp;
    bool called;
    float timer;
    float t;

    public bool Alive => hp > 0;
    public int Hp => hp;
    public int MaxHp => maxHp;

    public void Setup(Stage owner)
    {
        stage = owner;
        hp = maxHp;
        timer = 0.8f;
    }

    void FixedUpdate()
    {
        if (!Alive || !stage.Live)
            return;

        float dt = Time.fixedDeltaTime;
        float y = 1.66f;
        if (rb.position.y > y + 0.01f)
        {
            y = Mathf.MoveTowards(rb.position.y, y, speed * 1.8f * dt);
            rb.MovePosition(new Vector2(0, y));
            return;
        }

        if (!called)
        {
            called = true;
            stage.Helpers(helpers, summonCount);
        }

        t += dt;
        float x = Mathf.Sin(t * speed * 0.62f) * 2.45f;
        rb.MovePosition(new Vector2(x, y));

        timer -= dt;
        if (timer > 0)
            return;

        timer = volleyDelay;
        for (int i = 0; i < eggCount; i++)
        {
            float angle = (i - (eggCount - 1) * 0.5f) * volleySpread;
            stage.Egg(rb.position + Vector2.down * 0.7f, angle, eggSpeed);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Alive && stage.Live && other.TryGetComponent(out Ship ship))
            ship.Hit();
    }

    public void Missile()
    {
        Hit(Mathf.RoundToInt(maxHp * 0.18f));
    }

    public void Hit(int damage)
    {
        if (!Alive)
            return;

        hp = Mathf.Max(0, hp - damage);
        if (hp > 0)
        {
            stage.BossHit();
            return;
        }

        stage.BossGone(points);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
