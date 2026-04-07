using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CustomGoboStorage
{
    public const int MaxSlots = 16;
    public const int RequiredSize = 512;

    private const byte PngSignature0 = 137;
    private const byte PngSignature1 = 80;
    private const byte PngSignature2 = 78;
    private const byte PngSignature3 = 71;

    public static bool IsValidSlot(int slot)
    {
        return slot >= 1 && slot <= MaxSlots;
    }

    public static string GetFolderPath()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "CustomGobos");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        return folderPath;
    }

    public static string GetSlotFileName(int slot)
    {
        return $"slot{slot}.png";
    }

    public static string GetSlotPath(int slot)
    {
        return Path.Combine(GetFolderPath(), GetSlotFileName(slot));
    }

    public static List<int> GetFilledSlots()
    {
        var slots = new List<int>(MaxSlots);
        for (int slot = 1; slot <= MaxSlots; slot++)
        {
            if (File.Exists(GetSlotPath(slot)))
            {
                slots.Add(slot);
            }
        }

        return slots;
    }

    public static bool TrySaveSlotPng(int slot, byte[] pngBytes, out string error)
    {
        error = null;

        if (!IsValidSlot(slot))
        {
            error = $"Slot must be between 1 and {MaxSlots}.";
            return false;
        }

        if (pngBytes == null || pngBytes.Length == 0)
        {
            error = "Upload payload is empty.";
            return false;
        }

        if (!LooksLikePng(pngBytes))
        {
            error = "Only PNG files are accepted.";
            return false;
        }

        if (!PngHasAlphaChannel(pngBytes))
        {
            error = "PNG must include an alpha channel.";
            return false;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(pngBytes, markNonReadable: true))
            {
                error = "PNG payload could not be decoded.";
                return false;
            }

            if (texture.width != RequiredSize || texture.height != RequiredSize)
            {
                error = $"PNG must be {RequiredSize}x{RequiredSize} pixels.";
                return false;
            }
        }
        finally
        {
            UnityEngine.Object.Destroy(texture);
        }

        File.WriteAllBytes(GetSlotPath(slot), pngBytes);
        return true;
    }

    public static Texture2D LoadSlotTexture(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return null;
        }

        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            return null;
        }

        byte[] data = File.ReadAllBytes(path);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(data, markNonReadable: false))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        return texture;
    }

    public static bool TryDeleteSlotAndCompact(int slot, out string error)
    {
        error = null;
        if (!IsValidSlot(slot))
        {
            error = $"Slot must be between 1 and {MaxSlots}.";
            return false;
        }

        string slotPath = GetSlotPath(slot);
        if (!File.Exists(slotPath))
        {
            error = "Slot is already empty.";
            return false;
        }

        File.Delete(slotPath);
        for (int currentSlot = slot + 1; currentSlot <= MaxSlots; currentSlot++)
        {
            string sourcePath = GetSlotPath(currentSlot);
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            string targetPath = GetSlotPath(currentSlot - 1);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            File.Move(sourcePath, targetPath);
        }

        return true;
    }

    private static bool LooksLikePng(byte[] bytes)
    {
        return bytes.Length >= 8
               && bytes[0] == PngSignature0
               && bytes[1] == PngSignature1
               && bytes[2] == PngSignature2
               && bytes[3] == PngSignature3;
    }

    private static bool PngHasAlphaChannel(byte[] bytes)
    {
        if (bytes == null || bytes.Length < 26)
        {
            return false;
        }

        // PNG color type byte in IHDR payload: offset 25 (0-indexed)
        // 4 = grayscale+alpha, 6 = RGBA.
        byte colorType = bytes[25];
        return colorType == 4 || colorType == 6;
    }
}
