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
        {
            if (InAppPurchaseHelper.Instance.IsInitialized())
            {
                Setup(productData);
            }
            else
            {
                SetTextLoading();
                InAppPurchaseHelper.Instance.onInitializeComplete += OnIAPInit;
            }
        }
    }

    void OnIAPInit(bool success)
    {
        if (success)
        {
            Setup(productData);
        }
    }

    public void Setup(IAPProductData iAPProductData)
    {
        string priceText = InAppPurchaseHelper.Instance.GetPriceString(iAPProductData.ProductId);
        if (priceText != null)
        {
            textPrice.text = priceText;
        }
        else
        {
            SetTextLoading();
        }
    }

    void SetTextLoading()
    {
        textPrice.text = "Loading";
    }

    private void Reset()
    {
        textPrice = GetComponent<TMP_Text>();
    }

    private void OnDestroy()
    {
        InAppPurchaseHelper.Instance.onInitializeComplete -= OnIAPInit;
    }
}
