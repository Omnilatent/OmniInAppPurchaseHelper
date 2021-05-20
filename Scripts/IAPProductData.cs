using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[CreateAssetMenu(fileName = "ProductData", menuName = "IAP Product Data")]
public class IAPProductData : ScriptableObject
{
    public string ProductId { get => name; }
    public ProductType productType = ProductType.Consumable;
    public Payout[] payouts = new Payout[] { new Payout() };
}

[System.Serializable]
public class Payout
{
    public int quantity;

    [Header("Type: Currency = 1, Item = 2, Other = 3, NoAds = 4")]
    public PayoutType.Type type;

    public int subtype;
}