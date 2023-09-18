using DevionGames.Combat;
using DevionGames.Core;
using DevionGames.movement;
using DevionGames.Utils;
using System;
using UnityEngine;

namespace DevionGames.Control 
{
    public class AIcontrol : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f; //distancia de esfera azul
        [SerializeField] float suspiciusTime = 3f;
        [SerializeField] float aggroCooldownTime = 5; //Tiempo de enfriamiento agresivo
        [SerializeField] PatrolPath patrolPath; //asiganmos la patrulla
        [SerializeField] float waypointTolerance = 1f; //la tolerancia va ser por defecto 1 metro
        [SerializeField] float waypointDwellTime = 3f; //Tiempo de permanencia del waypoint
        [Range(0, 1)] //significa que cualquier fraccion de speed de patrulla solo debe ser de 0 a 1, por el Clamp01
        [SerializeField] float patrolSpeedFreaction = 0.2f; //*1 con fraction, aveces vamos a ir a una velocidad menor a la vel,maxima
        //*1//cambiamos la velocidad de los enemigos mientras hacemos patrullas
        [SerializeField] float shoutDistance = 5; // Distancia corta
        [SerializeField] GameObject menu; // Asigna el objeto del menú desde el inspector
        [SerializeField] float menuActivationDistance = 2f; // Distancia a la cual se abre el menú

        GameObject player; //hacemos una referencia para no tener que encontrarlo en cada frame
        Mover mover;
        CapturaEnemy captura;

        LazyValue<Vector3> guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity; //ultimo ataque del player
        float timeSinceArrivedAtWaipoint = Mathf.Infinity; //tiempo desde que llegó al waypoint
        float timeSinceAggravate = Mathf.Infinity; //el tiempo que cuando empezamos, osea nunca
        int currentWaipointIndex = 0; //decimos que el indice 0 es el waypoint en que esta el enemy
        //bool isAggravated = false; // Variable para controlar si el guardia está agravado o no
        //private bool isMenuOpened = false;
        private PlayerController playerController;
        private bool isChasingPlayer = false;


        private void Awake()
        {
            captura = GetComponent<CapturaEnemy>();

            player = GameObject.FindWithTag("Player");

            playerController = player.GetComponent<PlayerController>();

            mover = GetComponent<Mover>();

            guardPosition = new LazyValue<Vector3>(GetGuardPosition);
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            guardPosition.ForceInit();
        }

        private void Update()
        {

            if (isChasingPlayer)
            {
                ChasePlayer();
            }

            else if (IsAggrevated() && captura.CanAttack(player))
            { 
                AggrevateNearbyEnemy();
                AttackBehaviour();
            }

            else if (timeSinceLastSawPlayer < suspiciusTime)
            {
                //estado de sospecha
                SuspiciousBehaviour();
            }
            else// si no ven nada siguen en guardia
            {
                PatrolBehaviour();
            }

            UpdateTimers();

            if (menu.activeSelf && !IsAggrevated())
            {
                menu.SetActive(false);
            }
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition.value;
            if (patrolPath != null) //si no es nulo hacemos el patrullaje
            {
                if (AtWaitpoint()) //estamos en el waypoint
                {
                    timeSinceArrivedAtWaipoint = 0; //si el tiempo de llegada es 0 hacemos CycleWaipoint
                    CycleWaipoint(); //cambiamos de waypoint cuando patrullamos
                }
                nextPosition = GetCurrentWaypoint(); //obtenemos el waypoint actual
            }

            if (timeSinceArrivedAtWaipoint > waypointDwellTime) //si el tiempo de llegada es mayor que Tiempo de permanencia (waypoints) 
            {
                mover.StartMoveAction(nextPosition, patrolSpeedFreaction); //le decimos que vaya al siguiente waypoint
            }
        }

        private bool IsAggrevated() // lo hacemos bool para que no tengamos que buscar 2 veces al player
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggravate < aggroCooldownTime;
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0; //se reinicia cuando hacemos el ataque al player
            captura.Attack(player);
            AggrevateNearbyEnemy();

            if (!isChasingPlayer)
            {
                isChasingPlayer = true;
            }
        }

        private void ChasePlayer()
        {
            // Mover el guardia hacia el jugador
            mover.StartMoveAction(player.transform.position, 1f); // Utiliza la velocidad máxima (1f) para acercarse rápidamente

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= menuActivationDistance)
            {
                isChasingPlayer = false;

                // Detener el movimiento del guardia
                mover.Cancel();

                // Desactivar el control del jugador
                playerController.enabled = false;

                // Abre el menú aquí
                menu.SetActive(true);
            }
        }


        private void AggrevateNearbyEnemy() //Agravar al enemigo cercano
        {
            //SphereCastAll: si hay varios enemy juntos que nos ataquen, si le damos a uno,
            //Vector3.up, 0: para que la esfera no se mueva, y 0 para que no gane velocidad
            var hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (var hit in hits) //hacemos un bucle para cada uno
            {
                var ai = hit.collider.GetComponent<AIcontrol>(); //queremos encontrar compon. enemigos 
                if (ai == null) continue; //verificamos que lo que golpeamos tenga un AIcontrol
                if (ai == this) continue;
                //ai.Aggrevate(); //de lo contrario nos atacan
            }
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime; // incrementara esto en la cantidad que tomo en cada fps
            timeSinceArrivedAtWaipoint += Time.deltaTime; // incrementara esto en la cantidad que tomo en cada fps
            timeSinceAggravate += Time.deltaTime; // incrementara esto en la cantidad que tomo en cada fps, tiempo desde agravar
        }

        private bool AtWaitpoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint()); //nos da la distancia a los waypoint
            return distanceToWaypoint < waypointTolerance; //detectamos si la distancia al waypoint es menor a waypointTolerance
        }

        private void CycleWaipoint()
        {
            currentWaipointIndex = patrolPath.GetNextIndex(currentWaipointIndex); //vamos al siguiente waypoint
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaipointIndex); //decimos que estamos en el waypoint actual
        }

        private void SuspiciousBehaviour()
        {
            GetComponent<ActionProgramadora>().CancelCurrentAction(); //llamamos a CancelCurrentAction para que sospeche y regrese
        }

        private void OnDrawGizmosSelected() // call by unity
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance); //ponemos la esfera para ver el rango del enemigo
        }
    }
}
