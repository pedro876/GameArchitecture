using Architecture.HFSM;
using UnityEngine;

namespace Pit.Rope
{
    //Cuáles son los estados de una cuerda?
    // - idle: Se actualiza a sí misma completamente, aplicando spring forces sobre todo menos el origen.
    // - recover: Se libera el destino y se recupera cuerda hacia su origen, quitando joints poco a poco. Se aplican spring forces menos en la joint que se está recogiendo (index 1).
    // - rapel: Similar a recover, pero sin liberar el destino. Esto provoca que el origen se aproxime hacia el destino.
    // - launch: Se lanza el destino de la cuerda, añadiendo joints poco a poco conforme se aleja del origen. No se aplican fuerzas spring sobre nada (a no ser que se alcanza el número máximo de joints).
    // - hooked: El destino ha alcanzado una superficie a la que se engancha, se aplica fuerzas sobre todo menos destino y origen.
    // - free: Se libera la cuerda completamente, aplicando spring forces sobre todos los nodos (incluidos origen y destino).
    public class RopeHFSM : MonoBehaviour
    {
        [SerializeField] RopeContext ropeContext;
        private HFSM<RopeContext> _hfsm;

        private void Awake()
        {
            ropeContext.Initialize(transform);
            _hfsm = new HFSM<RopeContext>(ropeContext, new HState<RopeContext>[]
            {
                new RopeState_Idle(),
                new RopeState_Launch(),
                new RopeState_Hooked(),
            });
        }

        private void Start()
        {
            _hfsm.Enter();
        }

        private void FixedUpdate() => _hfsm.FixedUpdate();
        private void LateUpdate() => _hfsm.LateUpdate();

        [SerializeField] int parabolaQuality = 100;
        [SerializeField] float parabolaStep = 1f;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            //if (!Application.isPlaying) return;
            //if (ropeContext.MiddleJointCount > 0)
            //{
            //    float springLength = ropeContext.springLength / ropeContext.springLengthMultiplier;
            //    for (int i = 0; i < ropeContext.joints.Count - 1; i++)
            //    {
            //        Vector3 toNext = ropeContext.joints[i + 1].position - ropeContext.joints[i].position;
            //        float tension = toNext.magnitude - springLength;
            //        Gizmos.color = Color.Lerp(Color.blue, Color.red, tension / 3f);
            //        Gizmos.DrawLine(ropeContext.joints[i].position, ropeContext.joints[i + 1].position);
            //    }
            //}

            Vector3 s0 = transform.position;
            Vector3 v0 = transform.forward * ropeContext.launchVelocity;
            Vector3 a0 = Physics.gravity;//+Vector3.down*ropeContext.extraGravity;

            float t = parabolaStep;
            Vector3 s_t = s0;
            for(int i = 0; i < parabolaQuality; i++, t += parabolaStep)
            {
                Vector3 newS_t = s0 + v0 * t + (a0 * t * t) / 2;
                Gizmos.DrawLine(s_t, newS_t);
                s_t = newS_t;
            }
            
        }
    }
}
