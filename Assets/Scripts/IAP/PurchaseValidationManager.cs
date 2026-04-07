using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PurchaseValidationManager : MonoBehaviour
{
    [SerializeField] private UnityIapPurchaseGateway purchaseGateway;
    [SerializeField] private string validationEndpoint = string.Empty;
    [SerializeField, Min(0.001f)] private float validationIntervalHours = 24f;
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private Popup revocationPopup;
    [SerializeField] private Text revocationTitleText;
    [SerializeField] private Text revocationMessageText;

    private const string LastValidationUnixKey = "iap_last_validation_unix";
    private const string PendingRevocationsKey = "iap_pending_revocations";
    private const string FallbackDeviceIdKey = "iap_device_id";
    private static readonly string[] InvalidDeviceIds = { "unknown", "n/a", "null", "none", "unsupportedIdentifier" };
    private bool _validationInProgress;
    private string _resolvedDeviceId;

    private void Start()
    {
        HandleSuspiciousState();
        ApplyPendingRevocations();

        if (validateOnStart)
        {
            TryValidatePurchases();
        }
    }

    public void TryValidatePurchases()
    {
        Debug.Log("Trying validation...");
        if (_validationInProgress)
        {
            Debug.Log("Validation already in progress.. ");
            return;
        }

        if (!IsOnline())
        {
            Debug.Log("Trying validation but not online");
            return;
        }

        if (!ShouldValidate())
        {
            Debug.Log("Trying validation but should not validate");
            return;
        }

        if (purchaseGateway == null)
        {
            purchaseGateway = FindFirstObjectByType<UnityIapPurchaseGateway>();
        }

        if (purchaseGateway == null)
        {
            Debug.LogWarning("Purchase validation skipped: UnityIapPurchaseGateway not found.", this);
            return;
        }

        if (!purchaseGateway.IsUsingRealStore)
        {
            Debug.Log("Purchase validation skipped: non-Google Play store backend active.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(validationEndpoint))
        {
            Debug.LogWarning("Purchase validation skipped: endpoint URL is empty.", this);
            return;
        }

        StartCoroutine(ValidateAllPurchases());
    }

    private bool IsOnline()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private bool ShouldValidate()
    {
        long lastUnixSeconds = SaveLoadSettings.LoadLong(LastValidationUnixKey, 0L);
        Debug.Log($"Purchase validation: loaded last validation unix seconds = {lastUnixSeconds}.");
        if (lastUnixSeconds <= 0)
        {
            Debug.Log("Purchase validation: no previous validation time found; validation required.");
            return true;
        }

        DateTime lastValidationUtc;
        try
        {
            lastValidationUtc = DateTimeOffset.FromUnixTimeSeconds(lastUnixSeconds).UtcDateTime;
            Debug.Log($"Purchase validation: last validation UTC = {lastValidationUtc:O}.");
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.LogWarning(
                $"Purchase validation: stored last validation unix timestamp '{lastUnixSeconds}' is invalid; validation required.",
                this);
            return true;
        }

        DateTime utcNow = DateTime.UtcNow;
        double hoursSinceLastValidation = (utcNow - lastValidationUtc).TotalHours;
        DateTime nextValidationUtc = lastValidationUtc.AddHours(validationIntervalHours);
        bool shouldValidate = hoursSinceLastValidation >= validationIntervalHours;
        Debug.Log(
            $"Purchase validation: now={utcNow:O}, hoursSinceLastValidation={hoursSinceLastValidation:F4}, " +
            $"intervalHours={validationIntervalHours:F4}, nextValidationTimeUtc={nextValidationUtc:O}, shouldValidate={shouldValidate}.");

        return shouldValidate;
    }

    private IEnumerator ValidateAllPurchases()
    {
        Debug.Log("Purchase validation coroutine started");
        _validationInProgress = true;
        var validatedProducts = new HashSet<string>(StringComparer.Ordinal);
        var validProducts = new HashSet<string>(StringComparer.Ordinal);

        IReadOnlyList<UnityIapPurchaseGateway.OwnedProductReceipt> receipts = purchaseGateway.GetOwnedNonConsumableReceipts();

        Debug.Log($"Owned receipts count: {receipts.Count}");

        var currentEntitlements = CapabilityService.Instance?.GetAllActiveProducts();

        // 🔥 ADD FALLBACK HERE
        if (receipts.Count == 0 && currentEntitlements.Count > 0)
        {
            HandleMissingReceiptsFallback();
        }
        else
        {
            for (int i = 0; i < receipts.Count; i++)
            {
                UnityIapPurchaseGateway.OwnedProductReceipt receipt = receipts[i];
                Debug.Log("Purchase validation for receipt: " + receipt.ProductId + " json: " + receipt.ReceiptJson);
                if (string.IsNullOrWhiteSpace(receipt.ProductId) || string.IsNullOrWhiteSpace(receipt.ReceiptJson))
                {

                    continue;
                }

                string purchaseToken = GooglePlayReceiptParser.ExtractPurchaseToken(receipt.ReceiptJson);
                if (string.IsNullOrWhiteSpace(purchaseToken))
                {
                    Debug.LogWarning($"Purchase validation skipped for '{receipt.ProductId}': purchase token missing.", this);
                    continue;
                }

                ValidationResult result = ValidationResult.Invalid;
                yield return ValidateWithServer(receipt.ProductId, purchaseToken, value => result = value);
                HandleValidationResult(receipt.ProductId, result, validatedProducts, validProducts);
            }

            CapabilityService.Instance?.SyncValidatedEntitlements(validatedProducts, validProducts);
            SaveValidationTimestamp();
        }
        _validationInProgress = false;
    }

    private void HandleMissingReceiptsFallback()
    {
        Debug.LogWarning("No receipts found during validation. Checking for stale entitlements...");

        var currentEntitlements = CapabilityService.Instance?.GetAllActiveProducts();

        if (currentEntitlements == null || currentEntitlements.Count == 0)
        {
            Debug.Log("No entitlements active → nothing to revoke.");
            return;
        }

        Debug.LogWarning($"Found {currentEntitlements.Count} active entitlements but no receipts → marking suspicious.");

        // Instead of immediate revoke → graceful approach
        MarkSuspiciousState(currentEntitlements);
    }

    private const string SuspiciousStateKey = "iap_suspicious_state";

    private void MarkSuspiciousState(List<string> products)
    {
        SavePendingRevocations(products);
        SaveLoadSettings.SaveInt(SuspiciousStateKey, 1);
        SaveLoadSettings.Save();
    }

    private void HandleSuspiciousState()
    {
        int suspicious = SaveLoadSettings.LoadInt(SuspiciousStateKey, 0);
        if (suspicious == 0)
        {
            return;
        }

        Debug.LogWarning("Suspicious IAP state detected from previous session → revoking.");

        var products = LoadPendingRevocations();
        for (int i = 0; i < products.Count; i++)
        {
            CapabilityService.Instance?.RevokeProduct(products[i]);
        }

        ClearPendingRevocations();
        SaveLoadSettings.SaveInt(SuspiciousStateKey, 0);
        SaveLoadSettings.Save();

        ShowRevocationPopup(products);
    }

    public void ApplyPendingRevocations()
    {
        List<string> pending = LoadPendingRevocations();
        if (pending.Count == 0)
        {
            return;
        }

        for (int i = 0; i < pending.Count; i++)
        {
            CapabilityService.Instance?.RevokeProduct(pending[i]);
        }

        ClearPendingRevocations();
        ShowRevocationPopup(pending);
    }

    private static void HandleValidationResult(
        string productId,
        ValidationResult result,
        HashSet<string> validatedProducts,
        HashSet<string> validProducts)
    {
        if (string.IsNullOrWhiteSpace(productId) || validatedProducts == null || validProducts == null)
        {
            return;
        }

        validatedProducts.Add(productId);

        switch (result)
        {
            case ValidationResult.Valid:
            case ValidationResult.RevokedPending:
                validProducts.Add(productId);
                break;
        }
    }

    private IEnumerator ValidateWithServer(string productId, string purchaseToken, Action<ValidationResult> callback)
    {
        string deviceId = GetOrCreateDeviceId();
        var request = new ValidationRequest
        {
            productId = productId,
            purchaseToken = purchaseToken,
            deviceId = deviceId
        };

        string requestJson = JsonUtility.ToJson(request);
        Debug.Log($"Purchase validation request payload prepared for product '{productId}' with deviceId '{deviceId}'.");
        using (var webRequest = new UnityWebRequest(validationEndpoint, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(requestJson);
            webRequest.uploadHandler = new UploadHandlerRaw(payload);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Purchase validation request failed for '{productId}': {webRequest.error}", this);
                callback(ValidationResult.Invalid);
                yield break;
            }

            ValidationResponse response = JsonUtility.FromJson<ValidationResponse>(webRequest.downloadHandler.text);
            if (response == null)
            {
                callback(ValidationResult.Invalid);
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(response.productId) &&
                !string.Equals(response.productId, productId, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"Purchase validation response product mismatch. Requested '{productId}' but received '{response.productId}'.",
                    this);
                callback(ValidationResult.Invalid);
                yield break;
            }

            if (response.deviceIds != null && response.deviceIds.Count > 0 && !response.deviceIds.Contains(deviceId))
            {
                Debug.LogWarning(
                    $"Purchase validation response for '{productId}' did not include current deviceId '{deviceId}'. " +
                    "This may indicate stale worker cache data.",
                    this);
            }

            if (response.revoked)
            {
                AddPendingRevocation(productId);
                callback(ValidationResult.RevokedPending);
                yield break;
            }

            callback(response.valid ? ValidationResult.Valid : ValidationResult.Invalid);
        }
    }

    private void SaveValidationTimestamp()
    {
        long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTime savedUtc = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        DateTime nextValidationUtc = savedUtc.AddHours(validationIntervalHours);
        Debug.Log(
            $"Purchase validation: saving last validation time unix={unixSeconds}, utc={savedUtc:O}, nextValidationTimeUtc={nextValidationUtc:O}.");
        SaveLoadSettings.SaveLong(LastValidationUnixKey, unixSeconds);
        SaveLoadSettings.Save();
    }

    [Serializable]
    private class ValidationRequest
    {
        public string productId;
        public string purchaseToken;
        public string deviceId;
    }

    [Serializable]
    private class ValidationResponse
    {
        public string productId;
        public bool valid;
        public bool revoked;
        public List<string> deviceIds;
    }

    [Serializable]
    private class PendingRevocationsData
    {
        public List<string> productIds = new List<string>();
    }

    private enum ValidationResult
    {
        Invalid = 0,
        Valid = 1,
        RevokedPending = 2
    }

    private static List<string> LoadPendingRevocations()
    {
        string json = SaveLoadSettings.LoadString(PendingRevocationsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        PendingRevocationsData data = JsonUtility.FromJson<PendingRevocationsData>(json);
        if (data == null || data.productIds == null)
        {
            return new List<string>();
        }

        var deduped = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < data.productIds.Count; i++)
        {
            string productId = data.productIds[i];
            if (!string.IsNullOrWhiteSpace(productId))
            {
                deduped.Add(productId.Trim());
            }
        }

        return new List<string>(deduped);
    }

    private static void AddPendingRevocation(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            return;
        }

        List<string> pending = LoadPendingRevocations();
        if (pending.Contains(productId))
        {
            return;
        }

        pending.Add(productId);
        SavePendingRevocations(pending);
    }

    private static void ClearPendingRevocations()
    {
        SaveLoadSettings.SaveString(PendingRevocationsKey, string.Empty);
        SaveLoadSettings.Save();
    }

    private static void SavePendingRevocations(List<string> productIds)
    {
        var data = new PendingRevocationsData
        {
            productIds = productIds ?? new List<string>()
        };

        SaveLoadSettings.SaveString(PendingRevocationsKey, JsonUtility.ToJson(data));
        SaveLoadSettings.Save();
    }

    private void ShowRevocationPopup(List<string> revokedProducts)
    {
        if (revokedProducts == null || revokedProducts.Count == 0)
        {
            return;
        }
        string message = BuildRevocationMessage(revokedProducts);

        if (revocationTitleText != null)
        {
            revocationTitleText.text = "Purchase revoked";
        }

        if (revocationMessageText != null)
        {
            revocationMessageText.text = message;
        }

        if (revocationPopup != null)
        {
            revocationPopup.gameObject.SetActive(true);
            revocationPopup.Open();
            return;
        }

        Debug.LogWarning(message, this);
    }

    private string BuildRevocationMessage(List<string> revokedProducts)
    {
        if (revokedProducts == null || revokedProducts.Count == 0)
        {
            return "Some purchases were refunded and have been removed.";
        }

        var labels = new List<string>();
        var seenProducts = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < revokedProducts.Count; i++)
        {
            string productId = revokedProducts[i];
            if (string.IsNullOrWhiteSpace(productId))
            {
                continue;
            }

            string normalized = productId.Trim();
            if (!seenProducts.Add(normalized))
            {
                continue;
            }

            labels.Add(ResolveProductDisplayName(normalized));
        }

        if (labels.Count == 0)
        {
            return "Some purchases were refunded and have been removed.";
        }

        return labels.Count == 1
            ? $"\"{labels[0]}\" was refunded and has been removed."
            : $"{labels.Count} purchases were refunded and have been removed: {string.Join(", ", labels)}.";
    }

    private static string ResolveProductDisplayName(string productId)
    {
        CapabilityDatabase database = CapabilityDatabase.Instance;
        if (database == null)
        {
            return productId;
        }

        IReadOnlyList<CapabilityDefinition> definitions = database.CapabilityDefinitions;
        for (int i = 0; i < definitions.Count; i++)
        {
            CapabilityDefinition definition = definitions[i];
            if (definition == null)
            {
                continue;
            }

            if (string.Equals(definition.ProductId, productId, StringComparison.Ordinal))
            {
                return string.IsNullOrWhiteSpace(definition.DisplayTitle) ? productId : definition.DisplayTitle.Trim();
            }

            IReadOnlyList<string> alternates = definition.AdditionalProductIds;
            for (int alternateIndex = 0; alternateIndex < alternates.Count; alternateIndex++)
            {
                if (string.Equals(alternates[alternateIndex], productId, StringComparison.Ordinal))
                {
                    return string.IsNullOrWhiteSpace(definition.DisplayTitle) ? productId : definition.DisplayTitle.Trim();
                }
            }
        }

        return productId;
    }

    private string GetOrCreateDeviceId()
    {
        if (!string.IsNullOrWhiteSpace(_resolvedDeviceId))
        {
            return _resolvedDeviceId;
        }

        string systemDeviceId = NormalizeSystemDeviceId(SystemInfo.deviceUniqueIdentifier);
        if (!string.IsNullOrWhiteSpace(systemDeviceId))
        {
            _resolvedDeviceId = systemDeviceId;
            return _resolvedDeviceId;
        }

        string persistedFallbackId = SaveLoadSettings.LoadString(FallbackDeviceIdKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(persistedFallbackId))
        {
            _resolvedDeviceId = persistedFallbackId.Trim();
            return _resolvedDeviceId;
        }

        _resolvedDeviceId = Guid.NewGuid().ToString("N");
        SaveLoadSettings.SaveString(FallbackDeviceIdKey, _resolvedDeviceId);
        SaveLoadSettings.Save();
        Debug.LogWarning("System device identifier unavailable; generated persistent fallback IAP device ID.", this);
        return _resolvedDeviceId;
    }

    private static string NormalizeSystemDeviceId(string rawDeviceId)
    {
        if (string.IsNullOrWhiteSpace(rawDeviceId))
        {
            return string.Empty;
        }

        string trimmed = rawDeviceId.Trim();
        for (int i = 0; i < InvalidDeviceIds.Length; i++)
        {
            if (string.Equals(trimmed, InvalidDeviceIds[i], StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
        }

        return trimmed;
    }
}
