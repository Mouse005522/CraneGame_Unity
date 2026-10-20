using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIRaycaster
{
    public enum HandType
    {
        Right,Left
    }
    public class Collect
    {
        public HandType handType = HandType.Right;
        public Transform pointer;
        public Selectable selectable;
        public GameObject directLine;
        public PointerEventData pointerEvent;
        public Vector3 hitPosition;
        public bool isPress = false;
    }
    private static List<Collect> collects = new List<Collect>();
    static EventSystem CurrentEventSystem => EventSystem.current;
    static float lineCubeScale = 0.2f;

  
    public static void AddPointer(Transform pointer, HandType handType,GameObject lineCube = null)
    {
        if (CurrentEventSystem != null)
        {
            if (collects.Any(p => p.pointer == pointer) == false)
            {
                Collect c = new Collect();
                c.pointer = pointer;
                c.handType = handType;
                c.pointerEvent = new PointerEventData (CurrentEventSystem);
                SetupLineCube (c, lineCube);
                collects.Add(c);
            }
        }
    }

    private static void SetupLineCube(Collect collect,GameObject lineCube)
    {
        collect.directLine = lineCube != null ? lineCube : GameObject.CreatePrimitive(PrimitiveType.Cube);
        collect.directLine.SetActive(false);
        GameObject.Destroy(collect.directLine.GetComponent<Collider>());

        lineCubeScale = collect.directLine.transform.localScale.x;
    }

    /// <summary>
    /// 判斷指定手是否指著UI
    /// </summary>
    public static bool IsPointerUI(HandType handType)
    {
        var find = collects.Find(p => p.handType == handType);
        return find != null && find.selectable != null;
    }

    public static void UpdateRaycast()
    {
        foreach(var collect in collects)
        {
            RaycastHit hit;
            LayerMask maskExceptUI = 1 << LayerMask.NameToLayer("UI");
            // do a forward raycast to see if we hit a Button
            if (Physics.Raycast(collect.pointer.position, collect.pointer.forward, out hit, 500f, maskExceptUI))
            {
                if (collect.directLine != null)
                {
                    collect.directLine.transform.forward = collect.pointer.forward;
                    collect.directLine.transform.position = collect.pointer.position;
                    collect.directLine.transform.localScale = new Vector3(lineCubeScale, lineCubeScale, hit.distance * 0.5f);
                    collect.directLine.SetActive(true);
                }

                collect.hitPosition = hit.point;
                var button = hit.collider.GetComponent<Selectable>();
                if (button != null)
                {
                    if (collect.selectable != button)
                    {
                        if (collect.selectable != null)
                        {
                            collect.selectable.OnPointerExit(collect.pointerEvent);
                            ExitUI(collect);
                        }

                        collect.selectable = button;
                        collect.selectable.OnPointerEnter(collect.pointerEvent);
                        EnterUI(collect);
                    }
                }
                else
                {
                    if (collect.selectable != null)
                    {
                        collect.selectable.OnPointerExit(collect.pointerEvent);
                        ExitUI(collect);
                        collect.selectable = null;
                    }
                }
            }
            else
            {
                if (collect.directLine != null)
                {
                    collect.directLine.SetActive(false);
                }
                if (collect.selectable != null)
                {
                    collect.selectable.OnPointerExit(collect.pointerEvent);
                    ExitUI(collect);
                    collect.selectable = null;
                }
            }
            PressUI(collect);
        }
       

    }
    public static void SetPress(Collect collect, bool set)
    {
        collect.isPress = set;
        if (set == true && collect.selectable != null)
        {
            collect.selectable.OnPointerDown(collect.pointerEvent);
        }
        else if (set == false && collect.selectable != null)
        {
            collect.selectable.OnPointerUp(collect.pointerEvent);
        }
    }

    private static void EnterUI(Collect collect)
    {
        collect.selectable.GetComponents<IPointerEnterHandler> ().ToList ().ForEach (handler => handler.OnPointerEnter (collect.pointerEvent));
        //collect.selectable.SendMessage ("OnPointerEnter", collect.pointerEvent, SendMessageOptions.DontRequireReceiver);
    }

    private static void ExitUI(Collect collect)
    {
        collect.selectable.GetComponents<IPointerExitHandler> ().ToList ().ForEach (handler => handler.OnPointerExit (collect.pointerEvent));

        //collect.selectable.SendMessage ("OnPointerExit", collect.pointerEvent, SendMessageOptions.DontRequireReceiver);
    }

    private static void PressUI(Collect collect)
    {
        if (collect.isPress)
        {
            if (collect.selectable is IDragHandler) // for slider
            {
                collect.pointerEvent.position = collect.hitPosition;
                collect.selectable.OnPointerDown(collect.pointerEvent);
                if (collect.selectable.animator != null)
                {
                    collect.selectable.animator.SetTrigger(collect.selectable.animationTriggers.pressedTrigger);
                }
              
            }
        }
    }
    public static void ClickUI(HandType handType = HandType.Right)
    {
        foreach(var collect in collects)
        {
            if(handType != collect.handType)
            {
                continue;
            }

            if (collect.selectable != null)
            {
                collect.selectable.GetComponents<IPointerClickHandler> ().ToList ().ForEach (handler => handler.OnPointerClick (collect.pointerEvent));
                if (collect.selectable.animator != null)
                {
                    collect.selectable.animator.SetTrigger (collect.selectable.animationTriggers.selectedTrigger);
                }
            }

            if (collect.selectable != null)
            {
                var clickHandles = collect.selectable.GetComponents<IPointerClickHandler> ().ToList ();

                if (clickHandles.Count > 0)
                {
                    clickHandles.ForEach (handler =>
                    {
                        handler.OnPointerClick (collect.pointerEvent);
                    });

                    if (collect.selectable.animator != null)
                    {
                        collect.selectable.animator.SetTrigger (collect.selectable.animationTriggers.selectedTrigger);
                    }
                }

                var dragHandles = collect.selectable.GetComponents<IDragHandler> ().ToList ();

                if (dragHandles.Count > 0)
                {
                    if (collect.selectable != null)
                    {
                        collect.pointerEvent.position = collect.hitPosition;
                        collect.selectable.OnPointerDown (collect.pointerEvent);
                    }
                }
            }
        }
    }
}
