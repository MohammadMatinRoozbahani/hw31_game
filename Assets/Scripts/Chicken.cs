using UnityEngine;

public class Chicken : MonoBehaviour
{
    enum Mode { Entry, Form, Warning, Dive }

    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer skin;
    [SerializeField] Sprite warning;
    [SerializeField] int health = 2;
    [SerializeField] int points = 100;

    Stage stage;
    WaveDataSO wave;
    Mode mode;
    Sprite normal;
    Vector2 slot;
    Vector2 a;
    Vector2 b;
    Vector2 c;
    Vector2 dir;
    bool dives;
    bool gift;
    float t;
    float timeToEnter;
    float attack;
    int hp;

    public bool Alive { get; private set; }

    public void Setup(Stage owner, WaveDataSO data, Vector2 pos, int index, bool small)
    {
        stage = owner;
        wave = data;
        slot = pos;
        normal = skin.sprite;
        gift = !small && (index == 1 || index == 6);
        dives = !small && wave.diveEvery > 0 && (index + 1) % wave.diveEvery == 0;
        hp = health;
        if (small)
        {
            hp = Mathf.Max(1, health - 2);
            transform.localScale *= 0.6f;
        }

        Alive = true;
        float side = Mathf.Sign(slot.x);
        a = wave.path[0];
        b = wave.path[1];
        c = wave.path[2];
        a.x *= side;
        b.x *= side;
        c.x *= side;
        Enter(index * wave.gap);
    }

    void Enter(float wait)
    {
        mode = Mode.Entry;
        t = -wait;
        timeToEnter = (Vector2.Distance(a, b) + Vector2.Distance(b, c) +
                       Vector2.Distance(c, slot)) / wave.speed;
        rb.linearVelocity = Vector2.zero;
        rb.position = a;
        transform.position = a;
        rb.rotation = 0;
    }

    void FixedUpdate()
    {
        if (!Alive || !stage.Live)
            return;

        float dt = Time.fixedDeltaTime;
        switch (mode)
        {
            case Mode.Entry:
                EnterStep(dt);
                break;
            case Mode.Form:
                FormStep(dt);
                break;
            case Mode.Warning:
                attack -= dt;
                if (attack <= 0)
                    BeginDive();
                break;
            case Mode.Dive:
                DiveStep(dt);
                break;
        }
    }

    void EnterStep(float dt)
    {
        t += dt;
        if (t < 0)
            return;

        float u = Mathf.Clamp01(t / timeToEnter);
        Vector2 p = Vector2.Lerp(a, b, u);
        Vector2 q = Vector2.Lerp(b, c, u);
        Vector2 r = Vector2.Lerp(c, slot, u);
        rb.MovePosition(Vector2.Lerp(Vector2.Lerp(p, q, u), Vector2.Lerp(q, r, u), u));

        if (u >= 1)
        {
            mode = Mode.Form;
            t = 0;
            float wait = dives ? Random.Range(0.65f, 1.3f) : Random.Range(0.32f, 0.68f);
            attack = wait * wave.eggDelay;
        }
    }

    void FormStep(float dt)
    {
        t += dt;
        float x = Mathf.Sin(t * wave.speed * 0.32f) * 0.29f;
        float y = Mathf.Sin(t * 1.7f) * 0.10f;
        rb.MovePosition(slot + new Vector2(x, y));

        attack -= dt;
        if (attack > 0)
            return;

        if (dives)
        {
            mode = Mode.Warning;
            skin.sprite = warning;
            attack = 0.42f;
            return;
        }

        stage.Egg(rb.position + Vector2.down * 0.35f, Random.Range(-7f, 7f), wave.eggSpeed);
        attack = Random.Range(0.72f, 1.32f) * wave.eggDelay;
    }

    void BeginDive()
    {
        Vector2 delta = stage.ShipPos - rb.position;
        delta.y = Mathf.Min(-1.6f, delta.y);
        delta.x = Mathf.Clamp(delta.x, delta.y * 0.8f, -delta.y * 0.8f);
        dir = delta.normalized;
        rb.MoveRotation(Vector2.SignedAngle(Vector2.down, dir));
        mode = Mode.Dive;
        skin.sprite = normal;
    }

    void DiveStep(float dt)
    {
        Vector2 pos = rb.position + dir * ((wave.speed + 2.3f) * dt);
        if (pos.y < -Frame.Height - 0.65f || Mathf.Abs(pos.x) > Frame.Width + 0.65f)
            Enter(0.65f);
        else
            rb.MovePosition(pos);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!Alive || !stage.Live || !other.TryGetComponent(out Ship ship))
            return;

        ship.Hit();
        if (mode == Mode.Dive && stage.Live)
            Kill(false);
    }

    public void Hit(int damage)
    {
        if (!Alive)
            return;

        hp -= damage;
        if (hp <= 0)
            Kill(true);
    }

    public void Kill(bool reward)
    {
        if (!Alive)
            return;

        Alive = false;
        stage.ChickenGone(this, rb.position, reward, gift, points);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
