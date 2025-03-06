using UnityEngine;

namespace SG.Examples
{
    /// <summary> Selects one of two SG_TrackedHands based on which hand is connected first. </summary>
    public class SGEx_SelectHandModel : MonoBehaviour
    {
        public SG.Util.SGEvent ActiveHandConnect = new Util.SGEvent();
        public SG.Util.SGEvent ActiveHandDisconnect = new Util.SGEvent();

        [Header("Left Hand Components")]
        public SG_TrackedHand leftHand;
        public SG_HapticGlove leftGlove;

        [Header("Right Hand Components")]
        public SG_TrackedHand rightHand;
        public SG_HapticGlove rightGlove;

        public SG_TrackedHand ActiveHand { get; private set; }

        public bool Connected
        {
            get { return this.ActiveHand != null; }
        }

        public SG_HapticGlove ActiveGlove
        {
            get
            {
                if (this.ActiveHand != null && ActiveHand.RealHandSource != null && ActiveHand.RealHandSource is SG.SG_HapticGlove)
                {
                    return (SG.SG_HapticGlove)this.ActiveHand.RealHandSource;
                }
                return null;
            }
        }

        void Start()
        {
            Debug.Log("[SGEx_SelectHandModel] Start() called.");

            // 왼손과 오른손이 `null`인지 확인
            if (leftHand == null)
            {
                Debug.LogError("[SGEx_SelectHandModel] Left hand is NULL!");
            }
            else
            {
                Debug.Log("[SGEx_SelectHandModel] Left hand found.");
            }

            if (rightHand == null)
            {
                Debug.LogError("[SGEx_SelectHandModel] Right hand is NULL!");
            }
            else
            {
                Debug.Log("[SGEx_SelectHandModel] Right hand found.");
            }

            // 왼손과 오른손 모델 비활성화
            if (leftHand != null) leftHand.HandModelEnabled = false;
            if (rightHand != null) rightHand.HandModelEnabled = false;

            Debug.Log("[SGEx_SelectHandModel] Hand models disabled.");
        }

        void Update()
        {
            if (this.ActiveHand == null)
            {
                Debug.Log("[SGEx_SelectHandModel] No active hand detected. Checking for connections...");

                if (this.rightHand != null && this.rightHand.IsConnected())
                {
                    this.rightHand.HandModelEnabled = true;
                    if (this.leftHand != null) this.leftHand.gameObject.SetActive(false);
                    Debug.Log("[SGEx_SelectHandModel] Connected to a right hand!");
                    ActiveHand = this.rightHand;
                    ActiveHandConnect.Invoke();
                }
                else if (this.leftHand != null && this.leftHand.IsConnected())
                {
                    this.leftHand.HandModelEnabled = true;
                    if (this.rightHand != null) this.rightHand.gameObject.SetActive(false);
                    Debug.Log("[SGEx_SelectHandModel] Connected to a left hand!");
                    ActiveHand = this.leftHand;
                    ActiveHandConnect.Invoke();
                }
                else
                {
                    Debug.Log("[SGEx_SelectHandModel] No hands connected.");
                }
            }
            else
            {
                // 연결된 손이 연결 해제되었는지 확인
                if (ActiveHand.RealHandSource == null || !ActiveHand.RealHandSource.IsConnected())
                {
                    Debug.LogWarning("[SGEx_SelectHandModel] " + ActiveHand.name + " disconnected!");
                    if (this.rightHand != null) this.rightHand.HandModelEnabled = false;
                    if (this.rightHand != null) this.rightHand.gameObject.SetActive(true);
                    if (this.leftHand != null) this.leftHand.HandModelEnabled = false;
                    if (this.leftHand != null) this.leftHand.gameObject.SetActive(true);
                    ActiveHandDisconnect.Invoke();
                    ActiveHand = null;
                }
            }
        }
    }
}
