public static class WebUiPasswordProtection
{
    public static bool IsEnabled()
    {
        return SaveLoadSettings.LoadInt(SaveLoadSettings.WebUiPasswordEnabledKey, 0) == 1;
    }

    public static bool IsProtectionEnabled()
    {
        return IsEnabled();
    }

    public static string GetStoredPassword()
    {
        return SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
    }

    public static string GetPasswordForUnityUi()
    {
        return GetStoredPassword();
    }

    public static void SetEnabled(bool enabled)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.WebUiPasswordEnabledKey, enabled ? 1 : 0);
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    public static bool SetProtectionEnabled(bool enabled)
    {
        bool previous = IsEnabled();
        if (previous == enabled)
        {
            return false;
        }

        SetEnabled(enabled);
        return true;
    }

    public static bool HasConfiguredPassword()
    {
        return !string.IsNullOrWhiteSpace(SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty));
    }

    public static bool SetPassword(string rawPassword)
    {
        string trimmed = string.IsNullOrWhiteSpace(rawPassword) ? string.Empty : rawPassword.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
            SaveLoadSettings.SaveAndInvokeEvent();
            return false;
        }

        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordKey, trimmed);
        SaveLoadSettings.SaveAndInvokeEvent();
        return true;
    }

    public static void ClearPassword()
    {
        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
        SaveLoadSettings.SaveAndInvokeEvent();
    }

    public static bool VerifyPassword(string providedPassword)
    {
        string storedPassword = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
        if (string.IsNullOrWhiteSpace(storedPassword))
        {
            return false;
        }

        return string.Equals(storedPassword, providedPassword);
    }

    public static void MigrateLegacyPasswordIfNeeded()
    {
        if (HasConfiguredPassword())
        {
            return;
        }

        string legacy = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
        if (string.IsNullOrWhiteSpace(legacy))
        {
            return;
        }

        SetPassword(legacy);
    }
}
