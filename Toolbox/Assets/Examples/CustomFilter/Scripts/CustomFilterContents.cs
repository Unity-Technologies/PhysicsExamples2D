using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// Drops a row of boxes split into three groups, where boxes only make contact with others in their own group and pass straight through the rest.
/// Every box is one Physics Pose with a Physics Area Polygon whose callbacks come here, and this component decides for each touching pair whether a contact is created.
/// </summary>
/// <remarks>
/// A contact filter callback runs before a contact exists, so returning false means the pair is never solved at all, where the contact filter on a shape can only use its fixed categories and masks.
/// Every third box along the row is in the same group, so neighboring boxes are always in different groups.
/// Contact filter callbacks are switched on for the world while the example runs, and the world's own setting is put back when it unloads.
/// </remarks>
public sealed class CustomFilterContents : MonoBehaviour, PhysicsAreaCallbacks.IContactFilterCallback
{
    // Decides whether two touching shapes make contact, allowing it only when both are boxes of the same group, or when either one is not a box at all.
    // This runs while the simulation steps, possibly on another thread, so it only reads the box array, which never changes while the example runs.
    bool PhysicsAreaCallbacks.IContactFilterCallback.OnContactFilter2D(PhysicsAreaCallbacks.ContactFilterEvent contactFilterEvent)
    {
        var groupA = FindGroup(contactFilterEvent.areaA);
        var groupB = FindGroup(contactFilterEvent.areaB);

        if (groupA < 0 || groupB < 0)
            return true;

        return groupA == groupB;
    }

    private void OnEnable()
    {
        var world = PhysicsWorld.defaultWorld;
        m_WorldContactFilterCallbacks = world.contactFilterCallbacks;
        world.contactFilterCallbacks = true;
    }

    // The setting belongs to the world rather than to this scene, so it would otherwise stay switched on into whichever example is loaded next.
    private void OnDisable()
    {
        var world = PhysicsWorld.defaultWorld;
        world.contactFilterCallbacks = m_WorldContactFilterCallbacks;
    }

    // Returns the group of the specified area, or -1 when it is not one of the boxes.
    // The comparison is by reference because the usual equality check on a Unity object is not safe to call away from the main thread.
    private int FindGroup(PhysicsArea area)
    {
        if (area is null)
            return -1;

        for (var i = 0; i < m_Boxes.Length; ++i)
        {
            if (ReferenceEquals(m_Boxes[i], area))
                return i % GroupCount;
        }

        return -1;
    }

    #region Internal

    const int GroupCount = 3;

    [SerializeField] PhysicsArea[] m_Boxes = new PhysicsArea[0];

    bool m_WorldContactFilterCallbacks;

    #endregion
}
