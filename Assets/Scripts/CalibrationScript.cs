using UnityEngine;
using UnityEngine.UI;

public class CalibrationScript : MonoBehaviour
{
    public Button calibrationButton;
    public Transform targetObject;
    private Vector3 fixedPosition;

    void Start()
    {
        // 버튼 컴포넌트 가져오기
        calibrationButton = GameObject.Find("CalibrationButton").GetComponent<Button>();

        // 이동시킬 오브젝트 가져오기
        targetObject = GameObject.Find("TargetObject").GetComponent<Transform>();

        // 고정 위치 값 설정 (Inspector 창에서 확인한 값)
        fixedPosition = new Vector3(0f, 0f, 0f); // 예시: (0, 0, 0) 위치

        // 버튼 클릭 이벤트 리스너 추가
        calibrationButton.onClick.AddListener(Calibrate);
    }

    void Calibrate()
    {
        // 오브젝트를 고정 위치로 이동
        targetObject.position = fixedPosition;

        // 필요에 따라 추가적인 작업 수행 (예: 회전 초기화)
        // targetObject.rotation = Quaternion.identity;
    }
}