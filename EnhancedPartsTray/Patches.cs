using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Game;
using KSP.OAB;
using Shapes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnhancedPartsTray;

public static class Patches
{
    private const int Spacing = 4;
    private const int PartsPanelMaskToFilterPanelOffset = 75;
    private const int FilterPanelToGridLayoutOffset = 18;

    private static bool _first = true;
    private static Sprite _sizeTagSprite = Sprite.Create(new Texture2D(16, 16), new Rect(0, 0, 16, 16), new Vector2(8, 8));

    [HarmonyPatch(typeof(AssemblyFilterContainer), nameof(AssemblyFilterContainer.SetFilter))]
    public class ButtonsPatcher
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref GameObject ___buttonsContainer)
        {
            var gridLayoutGroup = ___buttonsContainer.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup is null)
            {
                return;
            }

            int offsetWidth = EnhancedPartsTray.ConfigWidth.Value - PartsPanelMaskToFilterPanelOffset - FilterPanelToGridLayoutOffset;
            int cellSize = (((offsetWidth - (Spacing * EnhancedPartsTray.ConfigNumCells.Value - 1) ) / EnhancedPartsTray.ConfigNumCells.Value)/2) * 2;
            
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            int width = (Spacing * 4) + ((Spacing + cellSize) * EnhancedPartsTray.ConfigNumCells.Value);
            Transform filterObject = ___buttonsContainer.transform.parent;
            if (filterObject is null)
            {
                return;
            }
            RectTransform filterObjectRect = filterObject.GetComponent<RectTransform>();
            filterObjectRect.sizeDelta = new Vector2(width, width);
        }
    }

    [HarmonyPatch(typeof(AssemblyPartsButton), nameof(AssemblyPartsButton.ShowSizeTag))]
    public class SizeTagPatcher
    {
        private static Dictionary<GameObject, string> _sizeStorage = new();
        
        [HarmonyPostfix]
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref Image ___sizeTagBG)
        {
            Vector3 tagScale = EnhancedPartsTray.ConfigTagScale.Value switch
            {
                IconType.Hidden => new Vector3(0, 0, 1),
                IconType.Dot => new Vector3(0.4f, 0.35f, 1),
                IconType.Small => new Vector3(0.75f, 0.75f, 1),
                _ => new Vector3(1, 1, 1),
            };
            
            GameObject sizeTag = ___sizeTagBG.gameObject;
            sizeTag.transform.localScale = tagScale;
            
            var img = sizeTag.GetComponent<Image>();
            if (img is not null)
            {
                if (_first)
                {
                    _sizeTagSprite = img.sprite;
                    _first = false;
                }

                if (EnhancedPartsTray.ConfigTagScale.Value == IconType.Dot)
                {
                    img.sprite = Sprite.Create(
                        DrawCircle(Color.white, 16, 16),
                        img.sprite.rect,
                        img.sprite.pivot
                    );
                }
                else
                {
                    img.sprite = _sizeTagSprite;
                }
            }

            
            foreach (Transform child in sizeTag.transform)
            {
                var text = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (text is null) 
                    continue;

                // Store original size tag
                if (text.text != ".")
                {
                    _sizeStorage[child.gameObject] = text.text;
                }

                if (EnhancedPartsTray.ConfigTagScale.Value == IconType.Dot)
                {
                    text.text = ".";
                    text.color = new Color(0, 0, 0, 0);
                }
                else
                {
                    text.text = _sizeStorage[child.gameObject];
                    text.color = Color.black;
                }
            }
            
            sizeTag.SetActive(tagScale != Vector3.zero);
        }

        private static Texture2D DrawCircle(Color color, int width, int height, int radius = 8)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var pixels = Enumerable.Repeat(new Color(0, 0, 0, 0), width * height).ToArray();
            tex.SetPixels(pixels);
            tex.Apply();
            
            float rSquared = radius * radius;

            int x = width / 2;
            int y = height / 2;

            for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    tex.SetPixel(u, v, color);

            tex.Apply();
            
            
            return tex;
        }
    }
}