using System;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.U2D.Physics.LowLevelExtras;

public class GetContactFiltering : MonoBehaviour
{
    private PhysicsShape m_PhysicsShape;
    
    private void Start()
    {
        m_PhysicsShape = GetComponent<SceneShape>().Shape;

    }

    private void OnEnable()
    {
        PhysicsEvents.PostSimulate += ShowContacts;
    }

    private void OnDisable()
    {
        PhysicsEvents.PostSimulate -= ShowContacts;
    }

    private void ShowContacts(PhysicsWorld physicsWorld, float deltaTime)
    {
        if (!physicsWorld.isDefaultWorld || !m_PhysicsShape.isValid)
            return;

        using var contacts = m_PhysicsShape.GetContacts();
        
        // Filtering code here.
    }
}
