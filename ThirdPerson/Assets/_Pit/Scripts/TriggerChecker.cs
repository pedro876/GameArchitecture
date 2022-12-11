using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TriggerChecker : MonoBehaviour
{
    public LayerMask layerMask;
    public HashSet<GameObject> objects;
    public GameObject lastObj;
    //public bool IsColliding => enabled && ((objects != null && objects.Count > 0) || _anyOverlap);
    public bool IsColliding => enabled && objects != null && objects.Count > 0;

    //private bool _anyOverlap = false;
    private bool _disabledTemporarily = false;
    private bool _destroyed = false;
    //private Rigidbody _rb;
    //private SphereCollider _sphere;

    //[SerializeField] int objCount;

    //private void Awake()
    //{
    //    _rb = GetComponent<Rigidbody>();
    //    if (_rb != null)
    //    {
    //        _sphere = GetComponent<SphereCollider>();
    //    }
    //}

    #region ENABLING

    public async void DisableForSeconds(float seconds)
    {
        if (_disabledTemporarily) return;
        _disabledTemporarily = true;
        gameObject.SetActive(false);
        await Task.Delay((int)(seconds * 1000f));
        if (_destroyed) return;
        gameObject.SetActive(true);
        _disabledTemporarily = false;
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
        _destroyed = true;
    }

    #endregion

    #region DETECTION

    //private void LateUpdate()
    //{
    //    objCount = objects.Count;
    //    if (_rb != null)
    //    {
    //        if (_rb.isKinematic && _sphere != null)
    //            _anyOverlap = Physics.CheckSphere(transform.position, _sphere.radius*transform.lossyScale.x, layerMask, QueryTriggerInteraction.Ignore);
    //        else _anyOverlap = false;
    //    }
    //}

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
            lastObj = obj;
        }
    }

    private void TryRemoveObject(GameObject obj)
    {
        if (objects.Contains(obj))
        {
            objects.Remove(obj);
        }
    }

    #endregion
}
