using System;
using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public class HandleIAPEventDummy : HandleIAPEventBase
    {
        protected override void OnToggleLoading(bool isLoading) { LogNotImplementedException(); }

        protected override void ShowErrorPopup(PurchaseResultArgs resultArgs) { LogNotImplementedException(); }

        public override void ShowMessagePopup(string message, bool isError = false) { LogNotImplementedException(); }

        protected override void ToggleShowAdOnResume(bool value) { }

        protected override void OnPurchaseRestored(bool success, string errorMessage)
        {
            var restoreMessage = string.Empty;
            if (success)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
                restoreMessage = "Restore Successful";
            }
            else
            {
                // Restoration failed.
                restoreMessage = $"Restore Failed. Error: {errorMessage}";
            }
            Debug.Log(restoreMessage);
        }

        void LogNotImplementedException()
        {
            Debug.LogException(new NotImplementedException("Cần thêm component thay thế HandleIAPEventDummy vào InAppPurchaseHelper để xử lý sự kiện từ IAP."));
        }
    }
}