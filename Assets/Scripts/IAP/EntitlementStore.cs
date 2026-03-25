using System;
using System.Collections.Generic;

public class EntitlementStore
{
    private readonly HashSet<string> _unlockedProductIds = new HashSet<string>(StringComparer.Ordinal);

    public void MarkUnlocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return;
        }

        _unlockedProductIds.Add(productId);
    }

    public bool IsUnlocked(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return false;
        }

        return _unlockedProductIds.Contains(productId);
    }
}
