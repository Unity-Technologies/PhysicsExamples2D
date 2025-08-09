using UnityEngine;

public interface IShapeColorProvider
{
    Color32 ShapeColorState { get; }
    bool IsShapeColorActive { get; }
}