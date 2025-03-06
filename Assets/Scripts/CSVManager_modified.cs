using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SG; // ✅ SG 네임스페이스 추가

public class CSVManager_modified : MonoBehaviour
{
    [Header("CSV Settings")]
    public string nameText;
    public float minSaveInterval = 0.02f; // 기본 저장 간격 설정
    public GameObject nova2Glove;
    
    private SG_HandModelInfo handModel;
    private Transform[][] fingerJoints;
    
    private List<string[]> data = new List<string[]>();
    private string[] tempData;
    private string fileName;
    private bool isCoroutineRunning = false;

    void Awake()
    {
        Debug.Log("[CSVManager] Awake() started.");

        fileName = nameText + ".csv";
        tempData = new string[27];

        // CSV 헤더 정의
        tempData[0] = "Name";
        tempData[1] = "Thumb_CMC"; tempData[2] = "Thumb_MCP"; tempData[3] = "Thumb_IP"; tempData[4] = "Thumb_FingerTip";
        tempData[5] = "Index_MCP"; tempData[6] = "Index_PIP"; tempData[7] = "Index_DIP"; tempData[8] = "Index_FingerTip";
        tempData[9] = "Middle_MCP"; tempData[10] = "Middle_PIP"; tempData[11] = "Middle_DIP"; tempData[12] = "Middle_FingerTip";
        tempData[13] = "Ring_MCP"; tempData[14] = "Ring_PIP"; tempData[15] = "Ring_DIP"; tempData[16] = "Ring_FingerTip";
        tempData[17] = "Pinky_MCP"; tempData[18] = "Pinky_PIP"; tempData[19] = "Pinky_DIP"; tempData[20] = "Pinky_FingerTip";
        tempData[21] = "Wrist_Position";
        tempData[22] = "Wrist_Rotation";
        tempData[23] = "TimeStamp";

        // Nova2Glove 확인
        if (nova2Glove == null)
        {
            Debug.LogError("[CSVManager] Nova2Glove is NOT assigned!");
            return;
        }

        // SG_TrackedHand 가져오기
        SG_TrackedHand trackedHand = nova2Glove.GetComponent<SG_TrackedHand>();
        if (trackedHand == null)
        {
            Debug.LogError("[CSVManager] SG_TrackedHand component not found on Nova2Glove.");
            return;
        }
        Debug.Log("[CSVManager] SG_TrackedHand found.");

        // Hand Model 가져오기
        GameObject handModelObject = trackedHand.handModel?.gameObject;
        if (handModelObject == null)
        {
            Debug.LogError("[CSVManager] Hand Model object not found on SG_TrackedHand.");
            return;
        }
        Debug.Log("[CSVManager] Hand Model found.");

        // SG_HandModelInfo 가져오기
        handModel = handModelObject.GetComponent<SG_HandModelInfo>();
        if (handModel == null)
        {
            Debug.LogError("[CSVManager] SG_HandModelInfo component not found on Hand Model.");
            return;
        }
        Debug.Log("[CSVManager] SG_HandModelInfo found.");

        // Finger Joint 데이터 가져오기
        fingerJoints = handModel.FingerJoints;
        if (fingerJoints == null || fingerJoints.Length == 0)
        {
            Debug.LogError("[CSVManager] FingerJoints array is null or empty.");
            return;
        }
        Debug.Log("[CSVManager] Finger joints loaded successfully.");

        // 손목 Transform 확인
        if (handModel.wristTransform == null)
        {
            Debug.LogError("[CSVManager] wristTransform is NULL!");
            return;
        }
        Debug.Log("[CSVManager] wristTransform found.");

        // 초기 CSV 저장
        data.Add((string[])tempData.Clone());
        SaveToFile();
    }

    void Update()
    {
        if (!isCoroutineRunning)
        {
            StartCoroutine(SaveCSVFile());
        }
    }

    IEnumerator SaveCSVFile()
    {
        isCoroutineRunning = true;

        tempData[0] = nameText;

        // 손가락 위치 및 회전 데이터 저장
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tempData[(i * 4) + j + 1] = $"\"Pos({fingerJoints[i][j].position.x},{fingerJoints[i][j].position.y},{fingerJoints[i][j].position.z})\"," +
                                            $"\"Rot({fingerJoints[i][j].rotation.eulerAngles.x},{fingerJoints[i][j].rotation.eulerAngles.y},{fingerJoints[i][j].rotation.eulerAngles.z})\"";
            }
        }

        // 손목 데이터 추가
        tempData[21] = $"\"Pos({handModel.wristTransform.position.x},{handModel.wristTransform.position.y},{handModel.wristTransform.position.z})\"";
        tempData[22] = $"\"Rot({handModel.wristTransform.rotation.eulerAngles.x},{handModel.wristTransform.rotation.eulerAngles.y},{handModel.wristTransform.rotation.eulerAngles.z})\"";

        // 타임스탬프 추가
        tempData[23] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 데이터 저장
        data.Add((string[])tempData.Clone());
        SaveToFile();

        yield return new WaitForSecondsRealtime(minSaveInterval);
        isCoroutineRunning = false;
    }
    private void SaveToFile()
    {
        string filepath = Application.persistentDataPath;
        string fullPath = Path.Combine(filepath, fileName);

        if (!Directory.Exists(filepath))
        {
            Directory.CreateDirectory(filepath);
        }

        StringBuilder sb = new StringBuilder();
        foreach (var line in data)
        {
            sb.AppendLine(string.Join(",", line));
        }

        File.WriteAllText(fullPath, sb.ToString());
        Debug.Log($"[CSVManager] File saved to: {fullPath}");
    }
}
