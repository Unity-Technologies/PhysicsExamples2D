using System;
using UnityEngine.LowLevelPhysics2D;

public static class PhysicsAPIExtensions
{
    #region Shape-Distance
    
    /// <summary>
    /// Get the minimum distance between shapes.
    /// </summary>
    /// <param name="physicsShape"></param>
    /// <param name="otherShape"></param>
    /// <param name="useRadii"></param>
    /// <returns></returns>
    public static PhysicsQuery.DistanceResult ShapeDistance(this PhysicsShape physicsShape, PhysicsShape otherShape, bool useRadii = true)
    {
        return PhysicsQuery.ShapeDistance(
            new PhysicsQuery.DistanceInput
            {
                shapeProxyA = physicsShape.CreateShapeProxy(),
                shapeProxyB = otherShape.CreateShapeProxy(),
                transformA = physicsShape.body.transform,
                transformB = otherShape.body.transform,
                useRadii = useRadii
            });
    }

    /// <summary>
    /// Get the minimum distance between a shape and a span of shapes.
    /// </summary>
    /// <param name="physicsShape"></param>
    /// <param name="otherShapes"></param>
    /// <param name="useRadii"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static PhysicsQuery.DistanceResult ShapeDistance(this PhysicsShape physicsShape, ReadOnlySpan<PhysicsShape> otherShapes, bool useRadii = true)
    {
        if (!physicsShape.isValid)
            throw new ArgumentException(nameof(physicsShape));
        
        var bestDistance = float.MaxValue;
        PhysicsQuery.DistanceResult bestResult = default;

        // Fetch shape A details.
        var shapeProxyA = physicsShape.CreateShapeProxy();
        var transformA = physicsShape.body.transform;
        
        // Iterate all the shapes.
        foreach (var otherShape in otherShapes)
        {
            if (!otherShape.isValid)
                throw new ArgumentException(nameof(physicsShape));

            // Skip if the same shape.
            if (otherShape == physicsShape)
                continue;
            
            // Query the distance.
            var result = PhysicsQuery.ShapeDistance(
                new PhysicsQuery.DistanceInput
                {
                    shapeProxyA = shapeProxyA,
                    transformA = transformA,
                    shapeProxyB = otherShape.CreateShapeProxy(),
                    transformB = otherShape.body.transform,
                    useRadii = useRadii
                });

            // Ignore if further away.
            if (result.distance >= bestDistance)
                continue;
            
            // Found a better distance.
            bestDistance = result.distance;
            bestResult = result;
        }

        return bestResult;
    }

    /// <summary>
    /// Get the minimum distance between a shape and all shapes attached to the specified body.
    /// </summary>
    /// <param name="physicsShape"></param>
    /// <param name="physicsBody"></param>
    /// <param name="useRadii"></param>
    /// <returns></returns>
    public static PhysicsQuery.DistanceResult ShapeDistance(this PhysicsShape physicsShape, PhysicsBody physicsBody, bool useRadii = true)
    {
        if (!physicsShape.isValid)
            throw new ArgumentException(nameof(physicsShape));

        if (!physicsBody.isValid)
            throw new ArgumentException(nameof(physicsBody));
        
        // Get all the body shapes.
        using var otherShapes = physicsBody.GetShapes();

        // Calculate the minimum distance.
        return physicsShape.ShapeDistance(otherShapes, useRadii);
    }

    #endregion
    
    #region Physics Objects in Auto-Properties
    
    public static PhysicsWorld Get(this PhysicsWorld obj) => obj;
    public static PhysicsBody Get(this PhysicsBody obj) => obj;
    public static PhysicsShape Get(this PhysicsShape obj) => obj;
    public static PhysicsJoint Get(this PhysicsJoint obj) => obj;
    public static PhysicsDistanceJoint Get(this PhysicsDistanceJoint obj) => obj;
    public static PhysicsFixedJoint Get(this PhysicsFixedJoint obj) => obj;
    public static PhysicsHingeJoint Get(this PhysicsHingeJoint obj) => obj;
    public static PhysicsIgnoreJoint Get(this PhysicsIgnoreJoint obj) => obj;
    public static PhysicsRelativeJoint Get(this PhysicsRelativeJoint obj) => obj;
    public static PhysicsSliderJoint Get(this PhysicsSliderJoint obj) => obj;
    public static PhysicsWheelJoint Get(this PhysicsWheelJoint obj) => obj;
    
    #endregion
}
