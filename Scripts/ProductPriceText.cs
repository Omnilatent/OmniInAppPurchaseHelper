using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductPriceText : MonoBehaviour
{
    [SerializeField] TMP_Text textPrice;

    private void Start()
    {
        textPrice.text = InAppPurchaseHelper.instance.GetPriceString(IAPProcessor.remove_ads);
    }

    private void Reset()
    {
        textPrice = GetComponent<TMP_Text>();
    }
}
