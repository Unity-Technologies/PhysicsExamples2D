---
name: unity-physicscore2d-geometry
description: Detailed information about Unity PhysicsCore2D geometry types and utilities
---

# Unity PhysicsCore2D Geometry Expert

This sub-skill provides detailed information about PhysicsCore2D geometry types and geometry utilities.

## Physics Geometry
Physics geometry are simple types which only contain the data to define a geometric area.
The geometry can be reused for various purposes such as creating PhysicsShape when they are added to a world, using the PhysicsWorld debug drawing feature by specifying geometry to draw or using geometry with physics query intersection tests for gameplay.
There are five geometric types available which directly relate to the only primitive types supposed by the PhysicsCore2D, these are:

### CircleGeometry
This geometry defines a circle comprising of a center and radius only.
This geometry defines a closed shape in that it has a closed interior.
This geometry is used when create a PhysicsShape of type PhysicsShape.ShapeType.Circle
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CircleGeometry.html

### CapsuleGeometry
This geometry defines two centers and a radius only.
The centers define the top and bottom center of the capsule, effectively like a line segment however the radius adds a radius to that line segment to form a capsule.
This geometry defines a closed shape in that it has a closed interior.
This geometry is used when create a PhysicsShape of type PhysicsShape.ShapeType.Capsule
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.CapsuleGeometry.html

### PolygonGeometry
This geometry defines a convex polygon with 3 to 8 sides (edges) only.
This geometry cannot be concave.
This geometry will automatically calculate the side (edge) normals automatically upon creation along with various other aspects used for performance such as the polygon center (centroid).
This geometry defines a closed shape in that it has a closed interior.
This geometry is used when create a PhysicsShape of type PhysicsShape.ShapeType.Polygon
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PolygonGeometry.html

### SegmentGeometry
This geometry is a line segment define by two positions.
This geometry defines an open shape in that it does not have an interior, indeed a line segment is effectively infitely thin.
This geometry is used when create a PhysicsShape of type PhysicsShape.ShapeType.Segment
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.SegmentGeometry.html

### ChainSegmentGeometry
This geometry is a line segment define by two positions however unlike the SegmentGeometry, it has two additional positions that define adjacent line segment connection information.
This geometry has two addition positions for line adjacency known as "ghost" positions. They to not define a line segment but hint to the physics solver how a collision with the line start/end vertexs should be handle.
This type of geometry can be created explicitly but is more often created automatically by using the PhysicsChain type.
This geometry defines an open shape in that it does not have an interior, indeed a line segment is effectively infitely thin.
This geometry is used when create a PhysicsShape of type PhysicsShape.ShapeType.ChainSegment
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.ChainGeometry.html

## Physics Geometry Utilities

### PhysicsComposer
A PhysicsComposer provides the ability to perform various types of geometric operations (boolean operations of OR, AND, NOT & XOR) on multiple physics geometry types.
A PhysicsComposer works by first being created then allowing multiple layers to be added where each layer is a specific geometry along with a composer operation. The composition can then be run and specified geometry output requested.
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsComposer.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsComposer.md

### PhysicsDestructor
A PhysicsDestructor provides the ability to perform two types of geometric destruction, Slicing (spliting geometry in two) or fragmentation (breaking into multiple fragments).
A PhysicsDestructor only works on PolygonGeometry although any physics geometry can be easily converted to PhysicsGeometry using the PhysicsComposer first.
You can find the API reference here: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/Unity.U2D.Physics.PhysicsDestructor.html
You can find an overview here: https://github.com/Unity-Technologies/PhysicsExamples2D/blob/6000.5/PhysicsCore2D/Projects/Primer/PhysicsDestructor.md
