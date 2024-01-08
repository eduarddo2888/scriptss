using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class AIPathWay : MonoBehaviour
{
    public List<GameObject> route; //una lista de objetos de juego que representan los puntos de la ruta que la IA del auto debe seguir
    public bool drawLine = true; //dibuja líneas entre los puntos de la ruta en el editor de Unity para visualizar la ruta

    void Awake() //si la lista route es null (es decir, no se ha inicializado), se crea una nueva lista.
    {
        if(route == null) //si no hay lista
            route = new List<GameObject>(); //se añade una
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying) //verifica si el juego no se está ejecutando.
        {
            if (route == null) //verifica si la lista route es null (no se ha inicializado) Si es así, entonces crea una nueva lista
            {
                route = new List<GameObject>(); // crea una nueva lista de objetos de juego y la asigna a route.
            }
            route.Clear(); //Esta línea borra todos los elementos de la lista route
            foreach (Transform node in transform) //cada hijo se representa como un Transform y se almacena en la variable node.
            {
                route.Add(node.gameObject); //agrega el objeto de juego del nodo actual a la lista route.
            }
        }
        if (route != null && route.Count > 1) //si no es nulo y es > a 1
        {
            if (drawLine) //se esta activada llamamos al metodo
            {
                DrawLine(); //llamamos al metodo
            }          
        }
    }

    private void DrawLine() 
    {
        int index = 0; //se utiliza para acceder a los elementos de la lista route.
        Vector3 lastPos = route[index].transform.position; //obtiene la posic. del primer punto de route, y la almacena en la variable lastPos
        for (int i = 0; i < route.Count; i++) //recorre todos los puntos de la ruta.
        {
            Debug.DrawLine(lastPos, route[index].transform.position);
            lastPos = route[index].transform.position;
            if (index == route.Count - 1)
            {
                Debug.DrawLine(lastPos, route[0].transform.position);
            }
            index++;
        }
    }
}
