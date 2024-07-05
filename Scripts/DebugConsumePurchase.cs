using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public class DebugConsumePurchase
    {
        public const string ppConsumeAllProductsNextOpen = "consumeAllProductsNextOpen";

        /// <summary>
        /// Must be called before IAP Helper initialization
        /// </summary>
        /// <param name="iapHelper"></param>
        public static void CheckConsumeAllIAPProducts(InAppPurchaseHelper iapHelper)
        {
            if (Debug.isDebugBuild && PlayerPrefs.GetInt(ppConsumeAllProductsNextOpen, 0) == 1)
            {
                iapHelper.ToggleDebugConsumeAllNonConsumable();
                PlayerPrefs.SetInt(ppConsumeAllProductsNextOpen, 0);
                PlayerPrefs.Save();
            }
        }
    }
}