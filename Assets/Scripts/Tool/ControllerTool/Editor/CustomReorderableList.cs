using System.Collections;
using UnityEngine;
using UnityEditorInternal;

public class CustomReorderableList : ReorderableList
{
    public CustomReorderableList(IList elements, System.Type elementType) : base(elements, elementType, true, true, false, false)
    {
        drawElementCallback = DrawElementCallback;
        drawElementBackgroundCallback = DrawElementBackgroundCallback;
        drawHeaderCallback = DrawHeaderCallback;
        drawNoneElementCallback = DrawEmptyElementCallback;
    }

    protected virtual void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
    }

    protected virtual void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
    }

    protected virtual void DrawHeaderCallback(Rect rect)
    {
    }

    protected virtual void DrawEmptyElementCallback(Rect rect)
    {
    }
}
