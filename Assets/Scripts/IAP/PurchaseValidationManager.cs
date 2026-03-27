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
    [SerializeField, Min(0.25f)] private float validationIntervalHours = 24f;
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private Popup revocationPopup;
    [SerializeField] private Text revocationMessageText;

    private const string LastValidationTicksKey = "iap_last_validation_ticks";
    private const string PendingRevocationsKey = "iap_pending_revocations";
    private bool _validationInProgress;

    private void Start()
    {
        ApplyPendingRevocations();

        if (validateOnStart)
        {
            TryValidatePurchases();
        }
    }

    public void TryValidatePurchases()
    {
        if (_validationInProgress)
        {
            return;
        }

        if (!IsOnline() || !ShouldValidate())
        {
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
        long lastTicks = SaveLoadSettings.LoadLong(LastValidationTicksKey, 0L);
        if (lastTicks <= 0)
        {
            return true;
        }

        DateTime lastValidationUtc;
        try
        {
            lastValidationUtc = new DateTime(lastTicks, DateTimeKind.Utc);
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }

        return (DateTime.UtcNow - lastValidationUtc).TotalHours >= validationIntervalHours;
    }

    private IEnumerator ValidateAllPurchases()
    {
        _validationInProgress = true;
        var validProducts = new HashSet<string>(StringComparer.Ordinal);

        IReadOnlyList<UnityIapPurchaseGateway.OwnedProductReceipt> receipts = purchaseGateway.GetOwnedNonConsumableReceipts();
        for (int i = 0; i < receipts.Count; i++)
        {
            UnityIapPurchaseGateway.OwnedProductReceipt receipt = receipts[i];
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
            HandleValidationResult(receipt.ProductId, result, validProducts);
        }

        CapabilityService.Instance?.SyncEntitlements(validProducts);
        SaveValidationTimestamp();
        _validationInProgress = false;
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

    private static void HandleValidationResult(string productId, ValidationResult result, HashSet<string> validProducts)
    {
        if (string.IsNullOrWhiteSpace(productId) || validProducts == null)
        {
            return;
        }

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
        var request = new ValidationRequest
        {
            productId = productId,
            purchaseToken = purchaseToken,
            deviceId = SystemInfo.deviceUniqueIdentifier
        };

        string requestJson = JsonUtility.ToJson(request);
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
        SaveLoadSettings.SaveLong(LastValidationTicksKey, DateTime.UtcNow.Ticks);
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

        const string message = "Some purchases were refunded and have been removed.";
        if (revocationMessageText != null)
        {
            revocationMessageText.text = message;
        }

        if (revocationPopup != null)
        {
            revocationPopup.Open();
            return;
        }

        Debug.LogWarning(message, this);
    }
}
