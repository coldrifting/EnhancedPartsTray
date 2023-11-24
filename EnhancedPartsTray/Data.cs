using KSP.OAB;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedPartsTray;

public static class Data
{
    private const int DefaultWidth = 455;
    private const int FlushOffset = 24;
    private const int Height = 1040;

    private const string HUDPrefix = "/OAB(Clone)/HUDSpawner/HUD/";
    private const string PartsPickerPath = HUDPrefix + "widget_PartsPicker";

    private const string UndoPanel = HUDPrefix + "widget_ToolBar/GRP-Undo-Redo";
    private const string PartPickerMask = HUDPrefix + "widget_PartsPicker/mask_PartsPicker";
    private const string PartPickerBg = HUDPrefix + "widget_PartsPicker/mask_PartsPicker/BG-panel";
    private const string OrientationCube = HUDPrefix + "widget_PartsPicker/orientation_Cube";
    private const string PartInfoPanel = HUDPrefix + "UI-Editor_Screen-Panel-Foreground";
    
    //private const int OrientationCubeOffset = 65;
    //private const string PartPickerBody = HUDPrefix + "mask_PartsPicker/GRP-Body";

    public static void Setup()
    {
    }

    public static void Apply()
    {
        int width = EnhancedPartsTray.ConfigWidth.Value;
        SetPos(PartPickerMask, -FlushOffset);
        SetPos(PartPickerMask, -508, false);
        SetRect(PartPickerMask, width);
        SetRect(PartPickerMask, Height, false);
        SetRect(PartPickerBg, width);
        SetRect(PartPickerBg, Height, false);

        int widthDelta = width - DefaultWidth;
        
        // Move Orientation cube out of the way
        SetPos(OrientationCube, 848);
        SetPos(OrientationCube, -980, false);
        SetScale(OrientationCube, 35);
        
        SetPos(UndoPanel, -425 + widthDelta - FlushOffset);
        SetPos(UndoPanel, 1026, false);
        
        SetPos(PartInfoPanel, 8 + widthDelta - FlushOffset - 1);
        SetPos(PartInfoPanel, 11, false);
        
        // Change 
        
        // Fix Redo Tooltip
        var undoGameObject = GameObject.Find(UndoPanel + "/KSP2ButtonText_ToolsBar-Undo");
        if (undoGameObject != null)
        {
            var undoImage = undoGameObject.GetComponent<Image>();
            var oldSprite = undoImage.sprite;
            
            var newSprite = Sprite.Create(
                CopyTexture(oldSprite.texture, true),
                oldSprite.rect,
                oldSprite.pivot
            );
            
            var redoGameObject = GameObject.Find(UndoPanel + "/KSP2ButtonText_ToolsBar-Redo");
            if (redoGameObject != null)
            {
                redoGameObject.transform.localScale = new Vector3(1, 1, 1);
                var redoGameObjectImage = redoGameObject.GetComponent<Image>();
                redoGameObjectImage.sprite = newSprite;
            }
        }
        
        RefreshPartsTray();
    }

    private static void SetPos(string name, float value, bool x = true)
    {
        var go = GameObject.Find(name);
        if (go is null)
        {
            return;
        }

        var pos = go.transform.localPosition;
        if (x)
        {
            pos.x = value;
        }
        else
        {
            pos.y = value;
        }

        go.transform.localPosition = pos;
    }

    private static void SetScale(string name, float value)
    {
        var go = GameObject.Find(name);
        if (go is null)
        {
            return;
        }

        go.transform.localScale = new Vector3(value, value, value);
    }

    private static void SetRect(string name, float value, bool x = true)
    {
        var go = GameObject.Find(name);
        if (go is null)
        {
            return;
        }

        var rect = go.GetComponent<RectTransform>();
        if (rect is null)
        {
            return;
        }

        var size = rect.sizeDelta;
        if (x)
        {
            size.x = value;
        }
        else
        {
            size.y = value;
        }

        rect.sizeDelta = size;
    }
    
    private static void RefreshPartsTray()
    {
        GameObject assemblyPartPickerGameObject = GameObject.Find(PartsPickerPath);
        if (assemblyPartPickerGameObject == null)
        {
            return;
        }
        AssemblyPartsPicker assemblyPartPicker = assemblyPartPickerGameObject.GetComponent<AssemblyPartsPicker>();
        if (assemblyPartPicker == null)
        {
            return;
        }
        assemblyPartPicker.RefreshPartsPicker();
    }

    private static Texture2D CopyTexture(Texture oldText, bool flipX = false, bool flipY = false)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            oldText.width,
            oldText.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        var scale = new Vector2(1, 1);
        var offset = new Vector2(0, 0);

        if (flipX)
        {
            scale -= new Vector2(2, 0);
            offset += new Vector2(1, 0);
        }
        
        if (flipY)
        {
            scale -= new Vector2(0, 2);
            offset += new Vector2(0, 1);
        }

        Graphics.Blit(oldText, renderTex, scale, offset);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(oldText.width, oldText.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}