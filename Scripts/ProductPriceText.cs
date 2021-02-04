using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductPriceText : MonoBehaviour
{
    [SerializeField] TMP_Text textPrice;
    [SerializeField] IAPProductData productData;

    private void Start()
    {
        textPrice.text = InAppPurchaseHelper.instance.GetPriceString(productData.ProductId);
    }

    private void Reset()
    {
        textPrice = GetComponent<TMP_Text>();
    }
}
