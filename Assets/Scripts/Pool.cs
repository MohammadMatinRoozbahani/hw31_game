using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    [SerializeField] Pooled[] items;
    [SerializeField] int warmCount = 12;

    readonly List<Pooled> active = new List<Pooled>();
    readonly Dictionary<Pooled, Queue<Pooled>> idle = new Dictionary<Pooled, Queue<Pooled>>();

    void Awake()
    {
        foreach (Pooled prefab in items)
        {
            Queue<Pooled> queue = new Queue<Pooled>();
            idle.Add(prefab, queue);
            for (int i = 0; i < warmCount; i++)
                queue.Enqueue(Make(prefab));
        }
    }

    Pooled Make(Pooled prefab)
    {
        Pooled item = Instantiate(prefab, transform);
        item.gameObject.SetActive(false);
        item.Setup(this, prefab);
        return item;
    }

    public void Spawn(Pooled prefab, Stage stage, Vector2 pos, Vector2 velocity, int hp)
    {
        if (!idle.TryGetValue(prefab, out Queue<Pooled> queue))
        {
            queue = new Queue<Pooled>();
            idle.Add(prefab, queue);
        }

        Pooled item = queue.Count > 0 ? queue.Dequeue() : Make(prefab);
        active.Add(item);
        item.Launch(stage, pos, velocity, hp);
    }

    public void Return(Pooled item)
    {
        if (!active.Remove(item))
            return;

        item.gameObject.SetActive(false);
        idle[item.Source].Enqueue(item);
    }

    public bool Has(Pooled.Kind kind)
    {
        foreach (Pooled item in active)
        {
            if (item.Type == kind)
                return true;
        }

        return false;
    }

    public void Clear(Pooled.Kind kind)
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i].Type == kind)
                Return(active[i]);
        }
    }
}
