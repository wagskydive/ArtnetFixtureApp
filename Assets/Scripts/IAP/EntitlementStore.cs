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

    public void MarkUnlocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return;
        }

        bool changed = _unlockedProductIds.Add(productId);
        if (changed && _persistLocally)
        {
            SaveToLocalStorage();
        }
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
        string raw = SaveLoadSettings.LoadString(SaveLoadSettings.IapEntitlementsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        string[] productIds = raw.Split('|');
        for (int i = 0; i < productIds.Length; i++)
        {
            string productId = productIds[i];
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            _unlockedProductIds.Add(productId.Trim());
        }
    }

    private void SaveToLocalStorage()
    {
        string raw = string.Join("|", _unlockedProductIds.OrderBy(id => id, StringComparer.Ordinal));
        SaveLoadSettings.SaveString(SaveLoadSettings.IapEntitlementsKey, raw);
        SaveLoadSettings.Save();
    }
}
