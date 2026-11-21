using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Frame : MonoBehaviour
{
    public const float Width = 4.4f;
    public const float Height = 5.5f;

    [SerializeField] RectTransform barA;
    [SerializeField] RectTransform barB;

    Camera cam;
    int width;
    int height;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        Fit();
    }

    void Update()
    {
        if (width != Screen.width || height != Screen.height)
            Fit();
    }

    void Fit()
    {
        width = Screen.width;
        height = Screen.height;
        float aspect = (float)Mathf.Max(1, width) / Mathf.Max(1, height);
        const float field = Width / Height;

        if (aspect >= field)
        {
            float w = field / aspect;
            float x = (1 - w) / 2;
            cam.rect = new Rect(x, 0, w, 1);
            float edge = Edge(x, width);
            Place(barA, Vector2.zero, new Vector2(edge, 1));
            Place(barB, new Vector2(1 - edge, 0), Vector2.one);
        }
        else
        {
            float h = aspect / field;
            float y = (1 - h) / 2;
            cam.rect = new Rect(0, y, 1, h);
            float edge = Edge(y, height);
            Place(barA, Vector2.zero, new Vector2(1, edge));
            Place(barB, new Vector2(0, 1 - edge), Vector2.one);
        }

        cam.aspect = field;
    }

    static float Edge(float gap, int size)
    {
        if (gap <= 0)
            return 0;

        float pad = 1f / Mathf.Max(1, size);
        return gap + Mathf.Min(pad, (0.5f - gap) * 0.5f);
    }

    static void Place(RectTransform bar, Vector2 min, Vector2 max)
    {
        if (bar == null)
            return;

        bar.anchorMin = min;
        bar.anchorMax = max;
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
        bar.gameObject.SetActive(max.x > min.x && max.y > min.y);
    }
}
