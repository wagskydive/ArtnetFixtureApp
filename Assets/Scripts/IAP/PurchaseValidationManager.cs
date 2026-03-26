using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PurchaseValidationManager : MonoBehaviour
{
    [SerializeField] private UnityIapPurchaseGateway purchaseGateway;
    [SerializeField] private string validationEndpoint = string.Empty;
    [SerializeField, Min(0.25f)] private float validationIntervalHours = 24f;
    [SerializeField] private bool validateOnStart = true;

    private const string LastValidationTicksKey = "iap_last_validation_ticks";
    private bool _validationInProgress;

    private void Start()
    {
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

            bool? isValid = null;
            yield return ValidateWithServer(receipt.ProductId, purchaseToken, value => isValid = value);

            if (isValid == true)
            {
                validProducts.Add(receipt.ProductId);
            }
        }

        CapabilityService.Instance?.SyncEntitlements(validProducts);
        SaveValidationTimestamp();
        _validationInProgress = false;
    }

    private IEnumerator ValidateWithServer(string productId, string purchaseToken, Action<bool> callback)
    {
        var request = new ValidationRequest
        {
            productId = productId,
            purchaseToken = purchaseToken
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
                callback(false);
                yield break;
            }

            ValidationResponse response = JsonUtility.FromJson<ValidationResponse>(webRequest.downloadHandler.text);
            bool isValid = response != null && response.valid;
            callback(isValid);
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
    }

    [Serializable]
    private class ValidationResponse
    {
        public string productId;
        public bool valid;
    }
}
