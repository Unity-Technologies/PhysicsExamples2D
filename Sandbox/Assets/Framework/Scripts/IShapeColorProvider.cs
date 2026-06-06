using UnityEngine;

public interface IShapeColorProvider
{
    Color ShapeColorState { get; }
    bool IsShapeColorActive { get; }
}