using Avaritia.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Avaritia.movement 
{
    public class Mover : MonoBehaviour, IAction
    {
        [SerializeField] float maxSpeed = 6;
        [SerializeField] float maxNavPathLength = 40;

        NavMeshAgent navMeshAgent;


        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
        void Update()
        {
            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination, float speedFraction) //StartMoveAction se activa cuango hagamos click
        {
            GetComponent<ActionProgramadora>().StartAction(this);
            MoveTo(destination, speedFraction); //llamamos a MoveTo para movernos al hacer click
        }

        public bool CanMoveTo(Vector3 destination) //es para que no ataquemos al enemy a mucha distancia
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path); //con esta linea tenemos la ruta 
            if (!hasPath) return false; //si no hay una es false y no vamos
            if (path.status != NavMeshPathStatus.PathComplete) return false; //si no es una ruta completa, es falso y no vamos
            if (GetPathLength(path) > maxNavPathLength) return false; //si el camino es demasiado largo es false y no podemos ir

            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction) //speedFraction es el nombre de float
        {
            navMeshAgent.destination = destination;
            //con Clamp01 decimos que cualquier valor que este entre () debe estar entre 0 y 1
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false; //para que camine
        }

        public void Cancel() //queremos que cuando vamos a un enemigo el player se detenga a cierto rango para atacar
        {
            navMeshAgent.isStopped = true; //para que deje de caminar
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = navMeshAgent.velocity; //aplicamos la velocidad del player usando el navMeshAgent
            Vector3 localVelocity = transform.InverseTransformDirection(velocity); //con InverseTransformDirection se vuelve velocidad local
            float speed = localVelocity.z; //lo usamos cuando se actualiza el animador, y es = a eso, queremos saber la velocidad del eje z
            GetComponent<Animator>().SetFloat("velocidaddeavance", speed); //agarramos el acomponente animator para tener el parametro forwardSpeed
        }

        private float GetPathLength(NavMeshPath path) //GetPathLength: Obtener longitud de ruta
        {
            float total = 0; //una variable acumuladora y comienza en 0
            if (path.corners.Length < 2) return total; //si < que 2, no podemos calcular la distancia entre cualquier cosa que < 2,return total  
            for (int i = 0; i < path.corners.Length - 1; i++) //este for incluye el index, para comparar 2 elementos del array al mismo tiempo
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]); //calculamos la distancia entre 2 puntos
            }
            return total;
        }
    }
}
