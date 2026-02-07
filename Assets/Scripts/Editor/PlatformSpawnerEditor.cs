using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformSpawner))]
public class PlatformSpawnerEditor : Editor
{
    SerializedProperty platformPrefab;
    SerializedProperty fallingSettings;
    SerializedProperty spikeSettings;

    private void OnEnable()
    {
        platformPrefab = serializedObject.FindProperty("platformPrefab");
        fallingSettings = serializedObject.FindProperty("fallingSettings");
        spikeSettings = serializedObject.FindProperty("spikeSettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. 기본 필드 그리기
        EditorGUILayout.PropertyField(platformPrefab);

        // 스포너의 기본 설정들 (interval, autoStart 등) 자동 그리기
        // (설정 변수들은 제외하고 그리기)
        DrawPropertiesExcluding(serializedObject, "m_Script", "platformPrefab", "fallingSettings", "spikeSettings");

        EditorGUILayout.Space(10);

        // 2. 프리팹 감지 로직
        GameObject prefab = platformPrefab.objectReferenceValue as GameObject;

        if (prefab != null)
        {
            // A. 떨어지는 발판 (FallingPlatform)
            if (prefab.GetComponentInChildren<FallingPlatform>(true) != null)
            {
                EditorGUILayout.LabelField("▼ 발판 설정 (Falling Platform)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                // FallingPlatformSettings 내부 변수 직접 그리기
                DrawProperty(fallingSettings, "fallSpeed", "낙하 속도");
                DrawProperty(fallingSettings, "lifetime", "수명 (초)");
                DrawProperty(fallingSettings, "groundLayer", "바닥 레이어");

                EditorGUILayout.EndVertical();
            }
            // B. 가시 (FallingSpike)
            else if (prefab.GetComponentInChildren<FallingSpike>(true) != null)
            {
                EditorGUILayout.LabelField("▼ 가시 설정 (Falling Spike)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                // FallingSpikeSettings 내부 변수 직접 그리기
                DrawProperty(spikeSettings, "detectionRange", "감지 거리");
                DrawProperty(spikeSettings, "detectionWidth", "감지 폭");
                DrawProperty(spikeSettings, "playerTag", "플레이어 태그");
                EditorGUILayout.Space(5);
                DrawProperty(spikeSettings, "shakeDuration", "흔들림 시간");
                DrawProperty(spikeSettings, "shakeAmount", "흔들림 강도");
                EditorGUILayout.Space(5);
                DrawProperty(spikeSettings, "fallSpeed", "낙하 속도");
                DrawProperty(spikeSettings, "lifetime", "낙하 후 수명");
                DrawProperty(spikeSettings, "groundLayer", "바닥 레이어");

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("지원하지 않는 프리팹입니다. (FallingPlatform/Spike 스크립트 없음)", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("프리팹을 넣어주세요.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 🟢 [헬퍼 함수] 내부 변수를 찾아서 그려주는 함수
    void DrawProperty(SerializedProperty parent, string propertyName, string label = null)
    {
        SerializedProperty prop = parent.FindPropertyRelative(propertyName);

        if (prop != null)
        {
            if (label != null)
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
            else
                EditorGUILayout.PropertyField(prop);
        }
        else
        {
            // 오타 났을 때 찾기 쉽게 에러 표시
            EditorGUILayout.LabelField($"Error: '{propertyName}' 못 찾음!", EditorStyles.miniLabel);
        }
    }
}