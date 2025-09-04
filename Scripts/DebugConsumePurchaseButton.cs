using UnityEngine;
using UnityEngine.UI;

namespace Omnilatent.InAppPurchase
{
    [RequireComponent(typeof(Button))]
    public class DebugConsumePurchaseButton : MonoBehaviour
    {
        /*[Tooltip("If false, only work in debug build")] [SerializeField]
        private bool _enableInProduction = false;*/

        protected virtual void Start()
        {
            GetComponent<Button>().onClick.AddListener(QueueConsumePurchase);
        }

        public virtual void QueueConsumePurchase()
        {
            if (Debug.isDebugBuild == false)
            {
                return;
            }

            DebugConsumePurchase.SetConsumeAllIAPProducts(true);
            Debug.Log($"Order to Consume all purchases received. Please restart the app twice for it to take effect.");
        }
    }
}