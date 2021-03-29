using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "ProductData", menuName = "IAP Product Data")]
public class IAPProductData : ScriptableObject
{
    public string ProductId { get => name; }
    public ProductType productType = ProductType.Consumable;
    [Tooltip("Price to display in dollar when getting product info from server failed")]
    public float defaultPrice;
    public Payout[] payouts = new Payout[] { new Payout() };

    Product GetProduct()
    {
        return InAppPurchaseHelper.instance.GetProduct(ProductId);
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
    public PayoutType.Type type;
}