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
        const int paddingPixels = 3;

        int topBound = 0;
        int bottomBound = 0;
        int leftBound = 0;
        int rightBound = 0;

        foreach (ShipPart part in buildArea.Parts)
        {
            leftBound = Mathf.Min(leftBound, part.position.x - 1);
            rightBound = Mathf.Max(rightBound, part.position.x + 1);
            bottomBound = Mathf.Min(bottomBound, part.position.y - 1);
            topBound = Mathf.Max(topBound, part.position.y + 1);
        }

        int atlasWidthPixels = (rightBound - leftBound + 1) * pixelsPerCell + paddingPixels * 2;
        int atlasHeightPixels = (topBound - bottomBound + 1) * pixelsPerCell + paddingPixels * 2;

        Texture2D atlasTexture = new Texture2D(atlasWidthPixels, atlasHeightPixels, TextureFormat.RGBA32, false);
        atlasTexture.filterMode = FilterMode.Point;

        Color32[] atlasPixels = new Color32[atlasWidthPixels * atlasHeightPixels];

        foreach (ShipPart part in buildArea.Parts)
        {
            int atlasBaseX = paddingPixels + (part.position.x - leftBound - 1) * pixelsPerCell;
            int atlasBaseY = paddingPixels + (part.position.y - bottomBound - 1) * pixelsPerCell;

            Sprite sourceSprite = part.image.sprite;
            Texture2D sourceTexture = sourceSprite.texture;
            Rect sourceRect = sourceSprite.textureRect;

            int sourceWidthPixels = (int)sourceRect.width;
            int sourceHeightPixels = (int)sourceRect.height;

            Color32[] sourcePixels = sourceTexture.GetPixels32();

            int rotationDegrees = Mathf.RoundToInt(part.Rotation) % 360;

            for (int sourceY = 0; sourceY < sourceHeightPixels; sourceY++)
            {
                for (int sourceX = 0; sourceX < sourceWidthPixels; sourceX++)
                {
                    int flippedX = sourceX;
                    int flippedY = sourceY;

                    if (part.xFlipped) flippedX = sourceWidthPixels - 1 - flippedX;
                    if (part.yFlipped) flippedY = sourceHeightPixels - 1 - flippedY;

                    int transformedX;
                    int transformedY;

                    switch (rotationDegrees)
                    {
                        case 90:
                            transformedX = flippedY;
                            transformedY = sourceWidthPixels - 1 - flippedX;
                            break;

                        case 180:
                            transformedX = sourceWidthPixels - 1 - flippedX;
                            transformedY = sourceHeightPixels - 1 - flippedY;
                            break;

                        case 270:
                            transformedX = sourceHeightPixels - 1 - flippedY;
                            transformedY = flippedX;
                            break;

                        default: // 0
                            transformedX = flippedX;
                            transformedY = flippedY;
                            break;
                    }

                    int sourcePixelX = (int)sourceRect.x + sourceX;
                    int sourcePixelY = (int)sourceRect.y + sourceY;

                    Color32 pixelColor = sourcePixels[sourcePixelY * sourceTexture.width + sourcePixelX];
                    if (pixelColor.a == 0) continue;

                    int atlasX = atlasBaseX + transformedX;
                    int atlasY = atlasBaseY + transformedY;

                    atlasPixels[atlasY * atlasWidthPixels + atlasX] = pixelColor;
                }
            }
        }

        atlasTexture.SetPixels32(atlasPixels);
        atlasTexture.Apply();

        Sprite combinedSprite = Sprite.Create(
            atlasTexture,
            new Rect(0, 0, atlasWidthPixels, atlasHeightPixels),
            new Vector2(0.5f, 0.5f),
            pixelsPerCell
        );

        #region testing
        GameObject previewObject = new GameObject("CombinedShip");
        previewObject.AddComponent<SpriteRenderer>().sprite = combinedSprite;

        float centerWorldX = (leftBound + rightBound + 1) * 0.5f;
        float centerWorldY = (bottomBound + topBound + 1) * 0.5f;
        previewObject.transform.position = new Vector3(centerWorldX, centerWorldY, 0f);
        #endregion

        return combinedSprite;
    }
}
