using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    enum State { Ready, Playing, Lost, Done }

    [SerializeField] LevelDataSO level;
    [SerializeField] Ship ship;
    [SerializeField] Pool pool;
    [SerializeField] SpriteRenderer backdrop;
    [SerializeField] Pooled egg;
    [SerializeField] Pooled gift;
    [SerializeField] Pooled missile;
    [SerializeField] Pooled burst;

    readonly List<Chicken> chickens = new List<Chicken>();
    GameManager game;
    Coroutine run;
    State state;

    public LevelDataSO Level => level;
    public int Round { get; private set; }
    public string Message { get; private set; }
    public Boss Target { get; private set; }
    public Vector2 ShipPos => ship.Pos;
    public WeaponDataSO Weapon => ship.Weapon;
    public bool Live => state == State.Ready || state == State.Playing;
    public bool CanShoot => state == State.Playing;
    public event Action Changed;

    void Start()
    {
        game = GameManager.I;
        game.Enter(level);
        backdrop.sprite = level.backdrop;
        run = StartCoroutine(Rounds());
    }

    IEnumerator Rounds()
    {
        for (int i = 0; i < level.waves.Length; i++)
        {
            Round = i + 1;
            WaveDataSO wave = level.waves[i];
            string label = $"Round {Round}";
            if (wave.kind == WaveDataSO.Kind.Asteroids)
                label += " / Asteroid belt";

            Set(State.Ready, label);
            yield return new WaitForSeconds(0.85f);

            Set(State.Playing, "");
            if (wave.kind == WaveDataSO.Kind.Asteroids)
                yield return Rocks(wave);
            else
                yield return Flock(wave);

            pool.Clear(Pooled.Kind.Egg);
            yield return new WaitForSeconds(0.85f);
        }

        Set(State.Ready, level.boss.gameObject.name);
        yield return new WaitForSeconds(1);
        Vector2 pos = new Vector2(0, Frame.Height + 1.3f);
        Target = Instantiate(level.boss, pos, Quaternion.identity);
        Target.Setup(this);
        Set(State.Playing, "");

        while (Target != null)
            yield return null;

        pool.Clear(Pooled.Kind.Gift);
        pool.Clear(Pooled.Kind.Laser);
        pool.Clear(Pooled.Kind.Star);
        if (!game.HasNext)
        {
            Set(State.Done, "Stages cleared\nEsc to menu");
            yield break;
        }

        Set(State.Done, $"Level {level.index} cleared");
        yield return new WaitForSeconds(1.6f);
        game.Next();
    }

    IEnumerator Flock(WaveDataSO wave)
    {
        for (int i = 0; i < wave.count; i++)
        {
            Vector2 pos = wave.slots[i % wave.slots.Length];
            AddChicken(wave, pos, i, false);
        }

        while (chickens.Count > 0)
            yield return null;
    }

    IEnumerator Rocks(WaveDataSO wave)
    {
        for (int i = 0; i < wave.count; i++)
        {
            Vector2 pos = wave.slots[i % wave.slots.Length];
            float x = UnityEngine.Random.Range(-0.72f, 0.72f);
            float speed = wave.speed * UnityEngine.Random.Range(0.87f, 1.13f);
            Spawn(wave.rock, pos, new Vector2(x, -speed), wave.rockHp);
            yield return new WaitForSeconds(wave.gap);
        }

        while (pool.Has(Pooled.Kind.Rock))
            yield return null;
    }

    void AddChicken(WaveDataSO wave, Vector2 pos, int i, bool small)
    {
        Chicken chicken = Instantiate(wave.chicken, transform);
        chickens.Add(chicken);
        chicken.Setup(this, wave, pos, i, small);
    }

    public void ChickenGone(Chicken chicken, Vector2 pos, bool reward, bool dropGift, int points)
    {
        chickens.Remove(chicken);
        if (!reward)
            return;

        game.AddScore(points);
        if (dropGift)
            Spawn(gift, pos, Vector2.down * 1.7f);
    }

    public void Spawn(Pooled prefab, Vector2 pos, Vector2 velocity, int hp = 1)
    {
        pool.Spawn(prefab, this, pos, velocity, hp);
    }

    public void Egg(Vector2 pos, float angle, float speed)
    {
        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.down;
        Spawn(egg, pos, dir * speed);
    }

    public void LaunchMissile(Vector2 pos)
    {
        pos.y += 0.55f;
        float top = Mathf.Min(pos.y + 4.4f, Frame.Height - 0.65f);
        float speed = (top - pos.y) / missile.Life;
        Spawn(missile, pos, Vector2.up * speed);
    }

    public void Detonate(Vector2 pos)
    {
        pos.y = Mathf.Min(pos.y, Frame.Height - 0.65f);
        Spawn(burst, pos, Vector2.zero);

        if (!Live)
            return;

        pool.Clear(Pooled.Kind.Egg);
        while (chickens.Count > 0)
            chickens[chickens.Count - 1].Kill(true);

        if (Target != null)
            Target.Missile();
    }

    public void Helpers(WaveDataSO wave, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float x = (i - (count - 1) * 0.5f) * 1.45f;
            AddChicken(wave, new Vector2(x, 0.55f), i, true);
        }
    }

    public void BossHit()
    {
        Changed?.Invoke();
    }

    public void BossGone(int points)
    {
        Target = null;
        pool.Clear(Pooled.Kind.Egg);
        while (chickens.Count > 0)
            chickens[chickens.Count - 1].Kill(false);

        game.AddScore(points);
        Changed?.Invoke();
    }

    void Set(State next, string message)
    {
        state = next;
        Message = message;
        Changed?.Invoke();
    }

    public void Lose()
    {
        StopCoroutine(run);
        run = null;
        pool.Clear(Pooled.Kind.Missile);
        pool.Clear(Pooled.Kind.Burst);
        Set(State.Lost, "Out of lives\nR to retry");
        Time.timeScale = 0;
    }

    public void Retry()
    {
        if (state == State.Lost)
            game.Retry();
    }

    public void Back()
    {
        if (run != null)
            StopCoroutine(run);

        game.Menu();
    }
}
