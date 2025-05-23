﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Omnilatent.InAppPurchase.EditorNS
{
    public static class MenuItemEditor
    {
        [MenuItem("Tools/Omnilatent/IAP Helper/Upgrade ProductData v1 to v2")]
        public static void UpgradeProductDataTypeSubtype()
        {
            var productDatas = Resources.LoadAll<IAPProductData>(InAppPurchaseHelper.dataFolder);
            for (int i = 0; i < productDatas.Length; i++)
            {
                productDatas[i].UpgradeTypeSubtype();
            }
        }
    }
}