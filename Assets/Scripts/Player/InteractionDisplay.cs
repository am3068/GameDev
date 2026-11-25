using UnityEngine;

internal sealed class InteractionDisplay
{
    private const string BookTitle = "Notes of a Fallen Survivor";
    private const string BookText =
        "I've been trapped on these islands for longer than I can remember.\n\n" +
        "There are four portals here, but none will open without the right gem.\n\n" +
        "I found a way to reach the first one. The guardian roaming this island carries the Forest Gem. " +
        "Defeat it and you can open the Forest Portal.\n\n" +
        "But don't fight it unprepared.\n\n" +
        "\u2014 A Fallen Survivor";

    private GUIStyle title;
    private GUIStyle body;
    private GUIStyle promptStyle;
    private Texture2D marker;

    public void DrawMarker(Vector2 center)
    {
        if (!marker)
        {
            const int resolution = 32;
            marker = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            const float midpoint = (resolution - 1) * .5f;
            var pixels = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                float distance = Mathf.Abs(new Vector2(x - midpoint, y - midpoint).magnitude - 10f);
                float alpha = Mathf.Clamp01(4f - distance);
                float brightness = Mathf.Clamp01(2f - distance);
                pixels[y * resolution + x] = new Color(brightness, brightness, brightness, alpha * .9f);
            }
            marker.SetPixels(pixels);
            marker.Apply(false, true);
        }

        float size = Mathf.Clamp(Screen.height * .019f, 12f, 22f);
        GUI.DrawTexture(new Rect(center.x - size / 2f, center.y - size / 2f, size, size), marker);
    }

    public void ReleaseMarker()
    {
        if (marker)
            Object.Destroy(marker);
    }

    public void Draw(bool bookOpen, string prompt)
    {
        if (!bookOpen && string.IsNullOrEmpty(prompt))
            return;

        if (title == null)
        {
            var ink = new Color(.16f, .13f, .10f);
            title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28, fontStyle = FontStyle.Bold, normal = { textColor = ink }
            };
            body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19, wordWrap = true, normal = { textColor = ink }
            };
            promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white }
            };
        }

        float scale = Mathf.Min(Screen.width / 800f, Screen.height / 600f);
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        var previousMatrix = GUI.matrix;
        GUI.matrix *= Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        if (bookOpen)
        {
            Fill(new Rect(0f, 0f, width, height), new Color(0f, 0f, 0f, .65f));
            var page = new Rect((width - 600f) / 2f, (height - 428f) / 2f, 600f, 428f);
            Fill(page, new Color(.90f, .84f, .70f));
            GUI.Label(new Rect(page.x + 32f, page.y + 24f, 536f, 42f), BookTitle, title);
            GUI.Label(new Rect(page.x + 32f, page.y + 78f, 536f, 326f), BookText, body);
            GUI.Label(new Rect(page.x, page.yMax + 8f, page.width, 36f), "[E] or [Esc] Close", promptStyle);
        }
        else
        {
            float promptWidth = promptStyle.CalcSize(new GUIContent(prompt)).x + 28f;
            var area = new Rect((width - promptWidth) / 2f, height / 2f + 36f, promptWidth, 34f);
            Fill(area, new Color(.05f, .05f, .04f, .86f));
            GUI.Label(area, prompt, promptStyle);
        }
        GUI.matrix = previousMatrix;
    }

    private static void Fill(Rect area, Color colour)
    {
        var previousColour = GUI.color;
        GUI.color = colour;
        GUI.DrawTexture(area, Texture2D.whiteTexture);
        GUI.color = previousColour;
    }
}
