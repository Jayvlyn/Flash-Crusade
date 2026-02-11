using UnityEngine;

public class PartSpriteCombiner
{
    BuildArea buildArea;
    int pixelsPerCell = 3;

    public PartSpriteCombiner(BuildArea buildArea)
    {
        this.buildArea = buildArea;
    }

    public Sprite CombinePartSprites()
    {
        int padding = 3;

        int topBound = 0;
        int bottomBound = 0;
        int leftBound = 0;
        int rightBound = 0;

        foreach(ShipPart part in buildArea.Parts)
        {
            leftBound   = Mathf.Min(leftBound,   part.position.x - 1);
            rightBound  = Mathf.Max(rightBound,  part.position.x + 1);
            bottomBound = Mathf.Min(bottomBound, part.position.y - 1);
            topBound    = Mathf.Max(topBound,    part.position.y + 1);
        }

        int width = (rightBound - leftBound + 1) * pixelsPerCell + padding * 2;
        int height = (topBound - bottomBound + 1) * pixelsPerCell + padding * 2;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        

        Rect rect = new Rect(0, 0, width, height);

        int originX = leftBound;
        int originY = bottomBound;

        // clean texture (probably not needed, already fresh texture)
        Color32[] clear = new Color32[width * height];
        tex.SetPixels32(clear);

        foreach (ShipPart part in buildArea.Parts)
        {
            int partPixelSize = pixelsPerCell * 3;

            int atlasBaseX = padding + (part.position.x - leftBound - 1) * pixelsPerCell;
            int atlasBaseY = padding + (part.position.y - bottomBound - 1) * pixelsPerCell;

            Sprite srcSprite = part.image.sprite;
            Texture2D srcTex = srcSprite.texture;
            Rect r = srcSprite.textureRect;

            int w = (int)r.width;
            int h = (int)r.height;

            Color32[] src = srcTex.GetPixels32();

            int rot = Mathf.RoundToInt(part.Rotation) % 360;

            for(int sy = 0; sy < h; sy++)
            {
                for (int sx = 0; sx < w; sx++) 
                {
                    int fx = sx;
                    int fy = sy;

                    if (part.xFlipped) fx = w - 1 - fx;
                    if (part.yFlipped) fy = h - 1 - fy;

                    int tx, ty;

                    switch (rot)
                    {
                        case 90:
                            tx = fy;
                            ty = w - 1 - fx;
                            break;

                        case 180:
                            tx = w - 1 - fx;
                            ty = h - 1 - fy;
                            break;

                        case 270:
                            tx = h - 1 - fy;
                            ty = fx;
                            break;

                        default: // 0
                            tx = fx;
                            ty = fy;
                            break;
                    }

                    int srcX = (int)r.x + sx;
                    int srcY = (int)r.y + sy;


                    Color32 c = src[srcY * srcTex.width + srcX];

                    if (c.a == 0) continue;

                    tex.SetPixel(atlasBaseX + tx, atlasBaseY + ty, c);
                }
            }
        }
        tex.Apply();


        Sprite combined = Sprite.Create(
            tex,
            rect,
            new Vector2(0.5f, 0.5f),
            pixelsPerCell
        );

        // for testing:
        GameObject go = new GameObject("CombinedShip");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = combined;

        float centerX = (leftBound + rightBound + 1) * 0.5f;
        float centerY = (bottomBound + topBound + 1) * 0.5f;

        go.transform.position = new Vector3(centerX, centerY, 0f);
        // end of testing

        return combined;
    }
}
