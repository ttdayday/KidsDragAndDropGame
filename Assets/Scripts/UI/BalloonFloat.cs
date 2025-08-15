using UnityEngine;
using UnityEngine.UI;

// Attach to a UI object with an Image to make it float upward with a gentle bob
public class BalloonFloat : MonoBehaviour
{
    public float riseSpeed = 80f;
    public float bobAmplitude = 10f;
    public float bobSpeed = 2f;
    public float lifetime = 3.5f;

    private RectTransform rt;
    private float t;
    private float startX;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null) startX = rt.anchoredPosition.x;
        var img = GetComponent<Image>();
        if (img != null) img.preserveAspect = true;
    }

    void Update()
    {
        if (rt == null) return;
        t += Time.deltaTime;
        var p = rt.anchoredPosition;
        p.y += riseSpeed * Time.deltaTime;
        p.x = startX + Mathf.Sin(t * bobSpeed) * bobAmplitude;
        rt.anchoredPosition = p;

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
