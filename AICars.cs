using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICars : MonoBehaviour
{
    public float accelerator; //se utiliza para controlar la aceleración del auto.
    public float turnPower = 1; //se utiliza para controlar la potencia de giro del auto.
    public float turnSpeed = 4; //se utiliza para controlar la velocidad de giro del auto.
    Rigidbody2D rb; //se utiliza para acceder y controlar el componente Rigidbody2D del auto.
    internal bool isAIMovement; //se utiliza para determinar si el movimiento del auto está controlado por la IA.
    private float friction = 1.5f; //se utiliza para controlar la fricción del auto.
    private float currentSpeed; //se utiliza para almacenar la velocidad actual del auto.
    private Vector2 curSpeed; // se utiliza para almacenar la velocidad actual del auto en dos dimensiones (x, y).
    float currentAcc; //se utiliza para almacenar la aceleración actual del auto.
    private bool isMoving; //se utiliza para determinar si el auto está en movimiento.

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentAcc = accelerator; //la aceler.almacenada es = a la aceleracion del auto
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            rb.AddForce(transform.up * currentAcc); //hace que el auto se mueva
        }
        if (!isMoving)
        {
            rb.drag = friction * 2; //cuando el auto drena se desliza, por eso la friction
        }
    }

    public void Movement(string turning, bool isPaddlePressed)
    {
        if (turning == "left") //Si turning es igual a “left”, entonces el auto gira a la izquierda. el giro es proporcional a currentSpeed.
            transform.Rotate(Vector3.forward * (currentSpeed + currentSpeed)); //si el auto rota a left, seguimos avanzando
        else if(turning == "right") //Si turning es igual a “right”, entonces el auto gira a la derecha el giro es proporcional a currentSpeed
            transform.Rotate(Vector3.forward * -(currentSpeed + currentSpeed)); //si el auto rota a right, seguimos avanzando
        if (isPaddlePressed) //Si el pedal está siendo presionado (isPaddlePressed es verdadero), 
        {
            isMoving = true; //entonces el auto está en movimiento (isMoving se establece en verdadero).
        }

        currentSpeed = curSpeed.magnitude / turnSpeed; 
        curSpeed = new Vector2(rb.velocity.x, rb.velocity.y); //se actualiza curSpeed para que sea igual a la velocidad actual del Rigidbody2D
    }
}
