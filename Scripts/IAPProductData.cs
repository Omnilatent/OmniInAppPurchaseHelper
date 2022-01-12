using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "ProductData", menuName = "IAP Product Data")]
public class IAPProductData : ScriptableObject
{
    public string ProductId { get => name; }
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
