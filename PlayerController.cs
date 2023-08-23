using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Avaritia.movement;

namespace Avaritia.Control 
{
    public class PlayerController : MonoBehaviour
    {
        [System.Serializable]
        struct CursorMapping //hace que aparezca en player,componente PlayerController, una lista a la cual ponemos los cursores
        {
            public CursorType type; //hacemos struct para hacer una lista, y public accedemos desde control class
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProyectionDistance = 1f; //Distancia máxima de proyección de malla de navegación
        [SerializeField] float raycastRadius = 1f; //el radio para atacar al enemy aumenta

        bool isDraggingUI = false; //para cuando arrastremos item, el player no se mueva

        private void Update()
        {
            if (InteractWithUI()) return;
            if (InteractWithComponent()) return;
            if (InteractWithMovement()) return;

            SetCursor(CursorType.None); //al final del update,no podemos interactuar con move o combat, y cursor es None
        }

        private bool InteractWithComponent() //intercartuar con componentes del suelo
        {
            RaycastHit[] hits = RayCastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>(); //llamamos al script IRaycastable
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType()); //consultamos el cursor que corresponde
                        return true;
                    }
                }
            }
            return false;
        }

        
        RaycastHit[] RayCastAllSorted() //la funcion es, si hay un item u objeto detras de un enemy, no halla bug en el cursor
        {
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius); //los objetos que recojamos tienen SphereCastAll

            float[] distances = new float[hits.Length]; //ordenamos la distancias 
            for (int i = 0; i < hits.Length; i++) //contruimos la matris de distances
            {
                distances[i] = hits[i].distance; //decimos que distances es = hits y la distancia 
            }

            Array.Sort(distances, hits); //ordenamos los hits
            return hits;
        }

        private bool InteractWithUI() //devolvemos true o false si pasamos la flecha sobre ui 
        {
            if (Input.GetMouseButtonUp(0)) //cuando soltamos el click ya se puede mover
            {
                isDraggingUI = false;
            }

            if (EventSystem.current.IsPointerOverGameObject()) //preguntamos: ¿Está el puntero sobre GameObject?
            {
                if (Input.GetMouseButtonDown(0)) //cuando el click del mouse esta bajo quitamos control de player
                {
                    isDraggingUI = true;
                }
                SetCursor(CursorType.UI);
                return true; //cuando intercatuamos 
            }
            if (isDraggingUI) //despues de IsPointerOverGameObject decimos que si isDraggingUI = true; tenemos que return true, si no es false 
            {
                return true;
            }

            return false; //y cuando ya no, y bloquea la declarasion if
        }

        private void SetCursor(CursorType type) //aca decimos que tipo de cursor usamos, que textura, y el vector2 ejes x/y
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto); //aplicamos la funcion del cursor
        }

        private CursorMapping GetCursorMapping(CursorType type) // regresa el CursorMapping a traves de un array
        {
            foreach (CursorMapping mapping in cursorMappings) //verificamos los puntos de asignacion
            {
                if (mapping.type == type) //verificamos que el type sea = al type que nos pasaron
                {
                    return mapping; //si es asi devolvemos el mapeo
                }
            }
            return cursorMappings[0]; //devolvemos el primer elemento de esta matriz
        }

        private bool InteractWithMovement()
        {
            Vector3 target; //target es un Vector3 default
            bool hasHit = RayCastNavMesh(out target); //out: información sobre la ubicación en la que ha llegado un raycast

            if (hasHit) //si golpeamos algo es true, entonces
            {
                if (!GetComponent<Mover>().CanMoveTo(target)) return false; //si no podemos ir a ese target, no interactuamos

                if (Input.GetMouseButton(0)) //si ponemos return true/false en otro lugar da error cuando hacemos click
                {
                    GetComponent<Mover>().StartMoveAction(target, 1f); //1f es valor maximo de clamp01
                }
                SetCursor(CursorType.Movement); //Movement lo creamos el el script CursorType
                return true; //si da con algo es true
            }
            return false; //si no es false
        }

        private bool RayCastNavMesh(out Vector3 target) //no podemos clickear a lugares donde no llega el NavMesh
        {
            target = new Vector3(); //decimos que target ahora es un Vector3 default, pero queremos que refleje una position, en nave mesh

            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit) return false; //si no hemos golpeado no obtuvimos informacion y no avanzamos
            NavMeshHit navMeshHit; //llamamos a NavMeshHit
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProyectionDistance, NavMesh.AllAreas);//*1
            //*1 hasCastToNavMesh: ha enviado a NavMesh
            if (!hasCastToNavMesh) return false; // si no hizo hasCastToNavMesh, devuelbe false de nuevo

            target = navMeshHit.position; //actualis. el taget para ir a la ubicasion correcta

            return true;
        }

        private static Ray GetMouseRay() // un ray basado en la pocision del mouse
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
