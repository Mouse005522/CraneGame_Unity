using UnityEditor;
using UnityEngine;

public abstract class EditorWindowExtender : EditorWindow
{
    public void HighlightObject(Object obj)
    {
        EditorGUIUtility.PingObject(obj);
        Selection.activeObject = obj;
    }

    #region 拖曳物件API
    protected void DragDrop()
    {
        switch (Event.current.type)
        {
            case EventType.DragUpdated:
                {
                    if (IsAcceptDrag(DragAndDrop.objectReferences))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    Event.current.Use();
                }
                break;
            case EventType.DragPerform:
                {
                    if (IsAcceptDrag(DragAndDrop.objectReferences))
                    {
                        DragAndDrop.AcceptDrag();
                        OnDragDropObjects(DragAndDrop.objectReferences);
                    }
                }
                break;
        }
    }

    protected virtual bool IsAcceptDrag(UnityEngine.Object[] objectReferences)
    {
        return true;
    }

    protected virtual void OnDragDropObjects(UnityEngine.Object[] objectReferences)
    {
    }
    #endregion
}
