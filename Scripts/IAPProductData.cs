using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ProductData", menuName = "IAP Product Data")]
public class IAPProductData : ScriptableObject
{
    public string ProductId
    {
        get
        {
            return name;
        }
    }

    [SerializeField] string appleAppStoreProductId;
    public string AppleAppStoreProductId { get => appleAppStoreProductId; }

    public ProductType productType = ProductType.Consumable;
    [Tooltip("Price to display in dollar when getting product info from server failed")]
    public float defaultPrice;

    [Tooltip("Default Subscription period by days, only for Subscription products")]
    public int defaultSubscriptionPeriodDays;

    public Payout[] payouts = new Payout[] { new Payout() };

    Product GetProduct()
    {
        return InAppPurchaseHelper.Instance.GetProduct(ProductId);
    }

    public string LocalizedPriceString
    {
        get
        {
            var product = GetProduct();
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }
            else
            {
                return $"${defaultPrice:n2}";
            }
        }
    }

    public void SetDefaultAppleAppstoreProductId()
    {
        appleAppStoreProductId = $"{Application.identifier}.{name}";
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < payouts.Length; i++)
        {
            if (payouts[i].payoutType == PayoutTypeEnum.NotSet && payouts[i].type != 0)
            {
                if (Enum.IsDefined(typeof(PayoutTypeEnum), (int)payouts[i].type))
                {
                    Debug.Log($"Detected deprecated [type] value in {name}. [payoutType] will be updated to match [type] value.");
                    payouts[i].payoutType = (PayoutTypeEnum)(int)payouts[i].type;

                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError($"Detected deprecated [type] value in {name}. Unable to match [payoutType] to [type] value. Please set [payoutType] to 'Other' and use [subtype] instead");
                }
            }
        }
    }
#endif
}

[System.Serializable]
public class Payout
{
    public int quantity;
    public PayoutTypeEnum payoutType;
    public string subtype;

    [System.Obsolete("Use payoutType instead")]
    [Tooltip("PayoutType.Type is deprecated. Do not use this field, use payoutType field instead.")]
    [ReadOnly] public PayoutType.Type type;
}
