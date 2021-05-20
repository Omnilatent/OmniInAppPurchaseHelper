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
        if (productData != null)
            Setup(productData);
    }

    public void Setup(IAPProductData iAPProductData)
    {
        textPrice.text = InAppPurchaseHelper.Instance.GetPriceString(iAPProductData.ProductId);
    }

    private void Reset()
    {
        textPrice = GetComponent<TMP_Text>();
    }
}
