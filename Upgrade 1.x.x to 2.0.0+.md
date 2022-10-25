# Upgrading IAP Helper from version 1.x.x to 2.0.0+
- Find all usage of field IAPProductData.payoutType and switch to use property IAPProductData.PayoutType (capitalize P in PayoutType).
- In toolbar, go to "Tools/Omnilatent/IAP Helper/Upgrade ProductData v1 to v2" to automatically migrate the following values to new field:
    + type (PayoutType.Type) to payoutType (PayoutTypeEnum).
    + _subtypeInt (int) to subtypeId (string).