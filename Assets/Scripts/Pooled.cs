using UnityEngine;

public class Pooled : MonoBehaviour
{
    public enum Kind { Laser, Egg, Gift, Rock, Star, Missile, Burst }

    [SerializeField] Kind kind;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float lifetime = 8;
    [SerializeField] Animation fx;

    Pool pool;
    Stage stage;
    float timer;
    int hp;

    public Kind Type => kind;
    public float Life => lifetime;
    public Pooled Source { get; private set; }

    public void Setup(Pool owner, Pooled source)
    {
        pool = owner;
        Source = source;
    }

    public void Launch(Stage owner, Vector2 pos, Vector2 velocity, int health)
    {
        stage = owner;
        hp = health;
        timer = lifetime;
        float angle = 0;
        if (kind != Kind.Rock && kind != Kind.Gift)
            angle = Vector2.SignedAngle(Vector2.up, velocity);

        transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, angle));
        gameObject.SetActive(true);
        rb.position = pos;
        rb.rotation = angle;
        rb.linearVelocity = velocity;
        rb.angularVelocity = kind == Kind.Rock ? Random.Range(-90f, 90f) : 0;

        if (fx != null)
        {
            fx.Stop();
            fx.Play();
            fx.Sample();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        Vector2 pos = rb.position;
        if (timer <= 0 || Mathf.Abs(pos.x) > Frame.Width + 1.25f ||
            pos.y > Frame.Height + 1.3f || pos.y < -Frame.Height - 1)
        {
            pool.Return(this);
            if (kind == Kind.Missile)
                stage.Detonate(pos);
        }
    }

    bool Hit(int damage)
    {
        hp -= damage;
        if (hp > 0)
            return false;

        if (kind == Kind.Rock)
            GameManager.I.AddScore(60);

        pool.Return(this);
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeSelf || !stage.Live)
            return;

        if (kind == Kind.Laser || kind == Kind.Star)
        {
            Shoot(other);
            return;
        }

        if (!other.TryGetComponent(out Ship ship))
            return;

        if (kind == Kind.Gift)
            GameManager.I.Gift();
        else
            ship.Hit();

        if (kind != Kind.Rock)
            pool.Return(this);
    }

    void Shoot(Collider2D other)
    {
        if (other.TryGetComponent(out Pooled item) && item.gameObject.activeSelf &&
            (item.Type == Kind.Egg || item.Type == Kind.Rock))
        {
            bool eggHit = item.Type == Kind.Egg && kind == Kind.Laser;
            if (item.Hit(hp) && eggHit)
                GameManager.I.EggShot();
        }
        else if (other.TryGetComponent(out Chicken chicken) && chicken.Alive)
        {
            chicken.Hit(hp);
        }
        else if (other.TryGetComponent(out Boss boss) && boss.Alive)
        {
            boss.Hit(hp);
        }
        else
        {
            return;
        }

        pool.Return(this);
    }
}
