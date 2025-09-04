using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Omnilatent.InAppPurchase
{
    [RequireComponent(typeof(Button))]
    public class RestorePurchaseButton : MonoBehaviour
    {
        protected virtual void Start()
        {
            GetComponent<Button>().onClick.AddListener(RestorePurchase);
        }

        public virtual void RestorePurchase()
        {
            InAppPurchaseHelper.Instance.RestorePurchases();
        }
    }
}