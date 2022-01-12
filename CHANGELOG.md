===
v2.0.0 (2022/01/12)
- Update to unity IAP 4.1.2:
- Change check receipt: don't use Cross Platform Validator anymore, use Product.hasReceipt instead.
- Restore transaction every time app is initialized using IGooglePlayStoreExtensions.
- Update payout: change PayoutType to enum instead of int struct.
- Subtype change from int to string.
- IAP button check disable if owned
- IAP button callback with purchase result args instead of bool.
- Always use standard purchasing module instance when create builder.
- Validate payout subtype when initializing in debug build.

===
v1.1.1 (2021/05/20)
- Add ExtraPackageImporter to import Extra scripts and Message popup

===
v1.1.0 (2021/03/30)
- Change PurchaseCompleteDelegate to use PurchaseResultArgs instead of passing 3 parameters
