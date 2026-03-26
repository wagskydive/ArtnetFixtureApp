using System;
using UnityEngine;

public static class GooglePlayReceiptParser
{
    public static string ExtractPurchaseToken(string receiptJson)
    {
        if (string.IsNullOrWhiteSpace(receiptJson))
        {
            return null;
        }

        try
        {
            ReceiptWrapper wrapper = JsonUtility.FromJson<ReceiptWrapper>(receiptJson);
            if (wrapper == null || string.IsNullOrWhiteSpace(wrapper.Payload))
            {
                return null;
            }

            PayloadWrapper payload = JsonUtility.FromJson<PayloadWrapper>(wrapper.Payload);
            if (payload == null || string.IsNullOrWhiteSpace(payload.json))
            {
                return null;
            }

            PurchaseData purchaseData = JsonUtility.FromJson<PurchaseData>(payload.json);
            if (purchaseData == null || string.IsNullOrWhiteSpace(purchaseData.purchaseToken))
            {
                return null;
            }

            return purchaseData.purchaseToken;
        }
        catch (Exception)
        {
            return null;
        }
    }

    [Serializable]
    private class ReceiptWrapper
    {
        public string Store;
        public string TransactionID;
        public string Payload;
    }

    [Serializable]
    private class PayloadWrapper
    {
        public string json;
        public string signature;
    }

    [Serializable]
    private class PurchaseData
    {
        public string purchaseToken;
        public string productId;
    }
}
