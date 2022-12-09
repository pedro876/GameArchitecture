using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TriggerChecker : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    public HashSet<GameObject> objects;
    public bool IsColliding => enabled && objects != null && objects.Count > 0;

    private bool disabledTemporarily = false;
    private bool destroyed = false;

    public async void DisableForSeconds(float seconds)
    {
        if (disabledTemporarily) return;
        disabledTemporarily = true;
        gameObject.SetActive(false);
        await Task.Delay((int)(seconds * 1000f));
        if (destroyed) return;
        gameObject.SetActive(true);
        disabledTemporarily = false;
    }

    private void OnEnable()
    {
        if (objects == null) objects = new HashSet<GameObject>();
        else objects.Clear();
    }

    private void OnDisable()
    {
        objects?.Clear();
    }

    private void OnDestroy()
    {
        destroyed = true;
    }

    private void OnTriggerEnter(Collider other) => TryAddObject(other.gameObject);
    private void OnTriggerExit(Collider other) => TryRemoveObject(other.gameObject);
    private void OnCollisionEnter(Collision collision) => TryAddObject(collision.gameObject);
    private void OnCollisionExit(Collision collision) => TryRemoveObject(collision.gameObject);

    private void TryAddObject(GameObject obj)
    {
        int otherLayer = obj.layer;
        int otherBitLayer = 1 << otherLayer;
        bool isValid = (layerMask | otherBitLayer) == layerMask;
        if (isValid)
        {
            objects.Add(obj);
        }
    }

    private void TryRemoveObject(GameObject obj)
    {
        if (objects.Contains(obj))
        {
            objects.Remove(obj);
        }
    }
}
