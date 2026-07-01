using UnityEditor;
using UnityEngine;

/// <summary>
/// QuestData 커스텀 인스펙터
/// targetEvent 종류에 따라 관련 필드만 표시
/// - COUNT 계열: targetValue
/// - ToolUpgraded: targetToolTier
/// - PenExpanded: targetPenStage
/// - FarmerHired / CourierHired: 식별 필드 없음
/// </summary>
[CustomEditor(typeof(QuestData))]
public class QuestDataEditor : Editor
{
    private SerializedProperty _questId;
    private SerializedProperty _title;
    private SerializedProperty _targetEvent;
    private SerializedProperty _targetValue;
    private SerializedProperty _targetToolTier;
    private SerializedProperty _targetPenStage;
    private SerializedProperty _reward;
    private SerializedProperty _arrowTargetId;

    private void OnEnable()
    {
        _questId        = serializedObject.FindProperty("questId");
        _title          = serializedObject.FindProperty("title");
        _targetEvent    = serializedObject.FindProperty("targetEvent");
        _targetValue    = serializedObject.FindProperty("targetValue");
        _targetToolTier = serializedObject.FindProperty("targetToolTier");
        _targetPenStage = serializedObject.FindProperty("targetPenStage");
        _reward         = serializedObject.FindProperty("reward");
        _arrowTargetId  = serializedObject.FindProperty("arrowTargetId");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_questId);
        EditorGUILayout.PropertyField(_title);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_targetEvent);

        var eventType = (QuestEventType)_targetEvent.enumValueIndex;
        DrawConditionFields(eventType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reward", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_reward);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Arrow", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_arrowTargetId);

        serializedObject.ApplyModifiedProperties();
    }

    // targetEvent 종류에 따라 관련 필드만 표시
    private void DrawConditionFields(QuestEventType eventType)
    {
        switch (eventType)
        {
            case QuestEventType.ToolUpgraded:
                EditorGUILayout.PropertyField(_targetToolTier);
                break;

            case QuestEventType.PenExpanded:
                EditorGUILayout.PropertyField(_targetPenStage);
                break;

            case QuestEventType.FarmerHired:
            case QuestEventType.CourierHired:
                // 대상이 하나뿐이라 식별 필드 불필요
                EditorGUILayout.HelpBox("이 액션은 완료 여부만 판정합니다. 추가 설정 없음.", MessageType.Info);
                break;

            default:
                // COUNT 계열
                EditorGUILayout.PropertyField(_targetValue);
                break;
        }
    }
}
