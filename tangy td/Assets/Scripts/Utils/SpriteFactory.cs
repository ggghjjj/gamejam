using UnityEngine;
using System.Collections.Generic;

public static class SpriteFactory
{
    private static Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    public static Sprite CreateSquare(Color color, int size = 32)
    {
        string key = $"sq_{color}_{size}";
        if (_cache.TryGetValue(key, out Sprite cached)) return cached;

        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        _cache[key] = sprite;
        return sprite;
    }

    public static Sprite CreateCircle(Color color, int size = 32)
    {
        string key = $"ci_{color}_{size}";
        if (_cache.TryGetValue(key, out Sprite cached)) return cached;

        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                pixels[y * size + x] = dist <= radius ? color : Color.clear;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        _cache[key] = sprite;
        return sprite;
    }
}
