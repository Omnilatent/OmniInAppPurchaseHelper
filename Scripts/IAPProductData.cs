using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Serialization;
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

    public string displayName;
    public Payout[] payouts = new Payout[] { new Payout() };

    Product GetProduct()
    {
        return InAppPurchaseHelper.Instance.GetProduct(ProductId);
    }

    public string DefaultPriceString
    {
        get
        {
            return $"${defaultPrice:n2}";
        }
    }

    public void SetDefaultAppleAppstoreProductId()
    {
        appleAppStoreProductId = $"{Application.identifier}.{name}";
    }

#if UNITY_EDITOR
    public void UpgradeTypeSubtype()
    {
        bool upgrading = false;
        for (int i = 0; i < payouts.Length; i++)
        {
            upgrading |= payouts[i].CheckUpdatePayoutType(name, out bool upgradeType, out bool upgradeSubtype);
            if (upgradeType)
            {
                Debug.Log($"Detected deprecated [type] value in {name}. [payoutType] will be updated to match [type] value.");
            }
            if (upgradeSubtype)
            {
                Debug.Log($"Detected deprecated [subtype] value in {name}. [subtypeId] will be updated to match [subtype] value.");
            }
        }

        if (upgrading)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
#endif
}

[System.Serializable]
public class Payout
{
    public int quantity;
    [SerializeField] private PayoutTypeEnum payoutType;
    public PayoutTypeEnum PayoutType { get => payoutType; set => payoutType = value; }
    [SerializeField] string subtypeId;

    [Header("Deprecated fields, do not use.")]

    [System.Obsolete("Use payoutType instead")]
    [Tooltip("PayoutType.Type is deprecated. Do not use this field, use payoutType field instead.")]
    [ReadOnly] public PayoutType.Type type;

    [System.Obsolete("Use subtypeId instead")]
    [FormerlySerializedAs("subtype")]
    [SerializeField] int _subtypeInt = 0;

    public string subtype { get => subtypeId; set => subtypeId = value; }

    /// <summary>
    /// Update payout type & subtype when upgrading from Omni IAP 1.1.1 to 2.0.0
    /// </summary>
    /// <returns>True if fields were updated</returns>
    public bool CheckUpdatePayoutType(object invokerName, out bool upgradeType, out bool upgradeSubtype)
    {
        upgradeType = false;
        upgradeSubtype = false;
#pragma warning disable CS0618 // Code required to update obsolete field, no need to warn
        if (PayoutType == PayoutTypeEnum.NotSet && type != 0)
        {
            if (Enum.IsDefined(typeof(PayoutTypeEnum), (int)type))
            {
                if (PayoutType != (PayoutTypeEnum)(int)type)
                {
                    PayoutType = (PayoutTypeEnum)(int)type;
                    upgradeType = true;
                }
            }
            else
            {
                Debug.LogError($"Detected deprecated [type] value '{type}' in {invokerName}. Unable to match [payoutType] to [type] value. Please set [payoutType] to 'Other' and use [subtype] instead");
            }
        }

        if (_subtypeInt != 0 && subtypeId != _subtypeInt.ToString())
        {
            subtypeId = _subtypeInt.ToString();
            upgradeSubtype = true;
        }
        return upgradeType || upgradeSubtype;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
