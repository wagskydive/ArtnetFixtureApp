using System;
using System.Collections.Generic;
using System.Linq;

public class EntitlementStore
{
    private readonly HashSet<string> _unlockedProductIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly bool _persistLocally;

    public EntitlementStore(bool persistLocally = false)
    {
        _persistLocally = persistLocally;

        if (_persistLocally)
        {
            LoadFromLocalStorage();
        }
    }

    public bool MarkUnlocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

        bool changed = _unlockedProductIds.Add(productId);
        if (changed && _persistLocally)
        {
            SaveToLocalStorage();
        }

        return changed;
    }

    public bool MarkLocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

        bool changed = _unlockedProductIds.Remove(productId);
        if (changed && _persistLocally)
        {
            SaveToLocalStorage();
        }

        return changed;
    }

    public bool IsUnlocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

        return _unlockedProductIds.Contains(productId);
    }

    public void ResetAll()
    {
        _unlockedProductIds.Clear();

        if (_persistLocally)
        {
            SaveToLocalStorage();
        }
    }

    public IReadOnlyCollection<string> GetUnlockedProductIds()
    {
        return _unlockedProductIds;
    }

    private void LoadFromLocalStorage()
    {
        string storedValue = SaveLoadSettings.LoadString(SaveLoadSettings.IapEntitlementsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return;
        }

        string decodedValue;
        bool wasEncrypted = IapEntitlementCrypto.TryDecrypt(storedValue, out decodedValue);

        if (!wasEncrypted)
        {
            // Backward compatibility: older builds stored IDs as plain text.
            decodedValue = storedValue;
        }

        bool loadedAny = LoadIdsFromRawString(decodedValue);

        // If we loaded from legacy plaintext, immediately migrate to encrypted format.
        if (loadedAny && !wasEncrypted)
        {
            SaveToLocalStorage();
        }
    }

    private bool LoadIdsFromRawString(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        bool loadedAny = false;
        string[] productIds = raw.Split('|');
        for (int i = 0; i < productIds.Length; i++)
        {
            string productId = productIds[i];
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            loadedAny |= _unlockedProductIds.Add(productId.Trim());
        }

        return loadedAny;
    }

    private void SaveToLocalStorage()
    {
        string raw = string.Join("|", _unlockedProductIds.OrderBy(id => id, StringComparer.Ordinal));
        string encrypted = IapEntitlementCrypto.Encrypt(raw);
        SaveLoadSettings.SaveString(SaveLoadSettings.IapEntitlementsKey, encrypted);
        SaveLoadSettings.Save();
    }
}
