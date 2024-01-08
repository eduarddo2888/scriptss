using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMomevent : MonoBehaviour
{
    List<GameObject> route; //una lista de objetos de juego que representan la ruta que el coche de la IA debe seguir
    private Vector3 nextPoint; //es la próxima posición en la ruta que el coche de la IA debe alcanzar.
    public GameObject AIPathHolder; //objeto de juego que contiene la ruta que el coche de la IA debe seguir.
    private bool isAccPressed = false; //indica si el acelerador está siendo presionado.
    AICars driver; //referencia al script del conductor de la IA que controla el coche.
    int totalWayPoints; //es el número total de puntos de ruta en la ruta
    int currentIndex; //es el índice del punto de ruta actual en la ruta
    private bool nextPos; //indica si el coche de la IA debe moverse al siguiente punto de ruta.
    private string turning; //indica la dirección en la que el coche de la IA está girando.
    AIPathWay wayPoint; //es el punto de ruta actual en la ruta
    internal Transform currentWayPoint; //es la transformación del punto de ruta actual en la ruta.

    // Start is called before the first frame update
    void Start()
    {
        driver = GetComponent<AICars>(); //obteniendo el componente `AICars` del objeto de juego actual y lo asignando a la variable driver
        if (AIPathHolder != null)
        {
            driver.isAIMovement = true;
            wayPoint = AIPathHolder.GetComponent<AIPathWay>();
            route = wayPoint.route;
            currentIndex = 0;
            totalWayPoints = route.Count;
            currentWayPoint = route[0].transform;
        }
    }
   
    void Update()
    {
        if (!driver.isAIMovement)
        if (!nextPos)
        {
            nextPoint = currentWayPoint.position;
            nextPos = true;
        }
        Vector3 relativeVector = transform.InverseTransformPoint(nextPoint);
        turning = null;

        if (relativeVector.x > 0.5f)
        {
            turning = "right";
        }
        else if (relativeVector.x < -0.5f)
        {
            turning = "left";
        }
        float dist = Vector2.Distance(transform.position, nextPoint);
        isAccPressed = true;
        if (dist < 2)
        {
            currentIndex++;
            if (currentIndex >= totalWayPoints)
            {
                currentIndex = 0;
            }
            currentWayPoint = route[currentIndex].transform;
            nextPos = false;
        }
    }

    private void FixedUpdate()
    {
        if (driver.isAIMovement)
        {
            driver.Movement(turning, isAccPressed);
        }
    }
}
