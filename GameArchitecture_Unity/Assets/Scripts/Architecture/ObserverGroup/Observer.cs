using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.ObserverGroup;
using UnityEditor;

public abstract class Observer<TEnum> : MonoBehaviour, IObserver<TEnum> where TEnum : System.Enum
{
    [SerializeField] bool subscribeAll = true;
    [SerializeField] bool subscribedWhileDisabled = false;
    [SerializeField] List<TEnum> eventsToSubscribe = new List<TEnum>();

    public string GetName() => name;
    public abstract void Notify(TEnum evt, object evtInfo);
    private bool subscribed = false;

    private void Awake()
    {
        Subscribe();
        DoAwake();
    }

    private void OnEnable()
    {
        Subscribe();
        DoOnEnable();
    }

    private void OnDisable()
    {
        if(!subscribedWhileDisabled)
            Unsubscribe();
        DoOnDisable();
    }

    private void OnDestroy()
    {
        if(!subscribedWhileDisabled)
            Unsubscribe();
        DoOnDestroy();
    }

    protected virtual void DoAwake() { }
    protected virtual void DoOnEnable() { }
    protected virtual void DoOnDisable() { }
    protected virtual void DoOnDestroy() { }

    private void Subscribe()
    {
        if (subscribed) return;
        subscribed = true;
        if (subscribeAll)
            ObserverGroup<TEnum>.SubscribeAll(this);
        else
            ObserverGroup<TEnum>.Subscribe(this, eventsToSubscribe);
    }

    private void Unsubscribe()
    {
        if(!subscribed) return;
        subscribed = false;
        if (subscribeAll)
            ObserverGroup<TEnum>.UnsubscribeAll(this);
        else
            ObserverGroup<TEnum>.Unsubscribe(this, eventsToSubscribe);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Observer<>), true)]
[CanEditMultipleObjects]
public class ObserverEditor : Editor
{
    SerializedProperty subscribeAll;
    SerializedProperty eventsToSubscribe;

    private GUIStyle backgroundStyle;
    private GUIStyle textStyle;
    private Texture2D texture;

    void OnEnable()
    {
        subscribeAll = serializedObject.FindProperty("subscribeAll");
        eventsToSubscribe = serializedObject.FindProperty("eventsToSubscribe");
        CreateHeaderBackgroundStyle(Color.black);
        //CreateHeaderLabelStyle();
    }

    private void CreateHeaderBackgroundStyle(Color color)
    {
        backgroundStyle = new GUIStyle();
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        backgroundStyle.normal.background = texture;

    }

    private void CreateHeaderLabelStyle()
    {
        if (textStyle != null) return;
        textStyle = GUI.skin.GetStyle("Label");
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontStyle = FontStyle.Bold;
    }

    public override void OnInspectorGUI()
    {
        CreateHeaderLabelStyle();
        serializedObject.Update();
        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.NextVisible(true);
        using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
        {
            EditorGUILayout.PropertyField(iterator, true);
        }

        WriteHeader("OBSERVER");

        bool wasEventsToSubscribe = false;

        while (iterator.NextVisible(false))
        {
            if (wasEventsToSubscribe) //and there are more properties
                EndObserverInspector();

            wasEventsToSubscribe = iterator.propertyPath == eventsToSubscribe.propertyPath;
            if (wasEventsToSubscribe && subscribeAll.boolValue)
                continue;
            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void EndObserverInspector()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        WriteHeader("OTHER PROPERTIES");
    }

    private void WriteHeader(string text)
    {
        /*var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontStyle = FontStyle.Bold;*/
        GUILayout.BeginHorizontal(backgroundStyle);
        GUILayout.Label(text, textStyle);
        GUILayout.EndHorizontal();
    }
}
#endif