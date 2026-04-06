using System;
using System.Collections.Generic;
using System.Linq;

public class EntitlementStore
{
    private readonly HashSet<string> _unlockedProductIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _consumablePurchaseCounts = new Dictionary<string, int>(StringComparer.Ordinal);
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

    public int RecordConsumablePurchase(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return 0;
        }

        int currentCount = GetConsumablePurchaseCount(productId);
        int updatedCount = currentCount + 1;
        _consumablePurchaseCounts[productId] = updatedCount;

        if (_persistLocally)
        {
            SaveToLocalStorage();
        }

        return updatedCount;
    }

    public bool TryConsume(string productId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(productId) || amount <= 0)
        {
            return false;
        }

        int currentCount = GetConsumablePurchaseCount(productId);
        if (currentCount < amount)
        {
            return false;
        }

        int updatedCount = currentCount - amount;
        if (updatedCount <= 0)
        {
            _consumablePurchaseCounts.Remove(productId);
        }
        else
        {
            _consumablePurchaseCounts[productId] = updatedCount;
        }

        if (_persistLocally)
        {
            SaveToLocalStorage();
        }

        return true;
    }

    public int GetConsumablePurchaseCount(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return 0;
        }

        return _consumablePurchaseCounts.TryGetValue(productId, out int count) ? count : 0;
    }

    public void ResetAll()
    {
        _unlockedProductIds.Clear();
        _consumablePurchaseCounts.Clear();

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

        LoadConsumableCountsFromLocalStorage();
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

        string consumablesRaw = BuildConsumablesRawString();
        SaveLoadSettings.SaveString(SaveLoadSettings.IapConsumablesKey, consumablesRaw);
        SaveLoadSettings.Save();
    }

    private void LoadConsumableCountsFromLocalStorage()
    {
        string raw = SaveLoadSettings.LoadString(SaveLoadSettings.IapConsumablesKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        string[] entries = raw.Split('|');
        for (int i = 0; i < entries.Length; i++)
        {
            string entry = entries[i];
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            string[] parts = entry.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            string productId = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            if (!int.TryParse(parts[1], out int count) || count <= 0)
            {
                continue;
            }

            _consumablePurchaseCounts[productId] = count;
        }
    }

    private string BuildConsumablesRawString()
    {
        if (_consumablePurchaseCounts.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            "|",
            _consumablePurchaseCounts
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}:{pair.Value}"));
    }
}
