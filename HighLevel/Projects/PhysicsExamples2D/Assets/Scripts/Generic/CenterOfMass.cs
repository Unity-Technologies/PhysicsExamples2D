using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    public Vector2 LocalCenterOfMass = Vector2.zero;

    private Rigidbody2D m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        UpdateCenterOfMass();
    }

    void OnValidate()
    {
        UpdateCenterOfMass();       
    }

    void OnDrawGizmos()
    {
        if (m_Rigidbody)
            Gizmos.DrawWireSphere(m_Rigidbody.worldCenterOfMass, 0.1f);
    }

    void UpdateCenterOfMass()
    {
        if (m_Rigidbody)
            m_Rigidbody.centerOfMass = LocalCenterOfMass;
    }
}
