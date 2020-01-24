using UnityEngine;
using UnityEditor;

/// <summary>
/// Follow a set of points using a Catmull–Rom spline curve fitting algorithm.
/// </summary>
public class FollowCurveTarget : MonoBehaviour
{
	public float CurveDuration = 5.0f;
	public Color CurveColor = Color.white;
	public Vector2[] CurvePoints = { Vector2.zero };

	private TargetJoint2D m_Joint;
	private float m_CurveTime;

	// Use this for initialization
	void Start ()
	{
		m_Joint = GetComponent<TargetJoint2D> ();

		if (CurvePoints.Length > 0)
			m_Joint.target = transform.position + (Vector3)CurvePoints[0];
	}

	void OnValidate ()
	{
        if (CurveDuration < Mathf.Epsilon)
            return;

		while (m_CurveTime > CurveDuration)
			m_CurveTime -= CurveDuration;
	}

	void FixedUpdate ()
	{
        if (CurveDuration < Mathf.Epsilon)
            return;

		var curvePointCount = CurvePoints.Length;
		if (curvePointCount < 2)
		{
			m_CurveTime = 0.0f;
			m_Joint.target = transform.position;
			return;
		}

		m_CurveTime += Time.deltaTime;

		while (m_CurveTime > CurveDuration)
			m_CurveTime -= CurveDuration;

		var start = (int)((curvePointCount / CurveDuration) * m_CurveTime);
		var end = start < (curvePointCount-1) ? start+1 : 0;
		var previous = start > 0 ? start-1 : curvePointCount-1;
		var next = end < (curvePointCount-1) ? end+1 : 0;

		var unitDuration = CurveDuration / curvePointCount;
		var unitElapsedTime = m_CurveTime - (start * unitDuration);

		var samplePoint = CatmullRom (CurvePoints[previous], CurvePoints[start], CurvePoints[end], CurvePoints[next], unitElapsedTime, unitDuration);
		m_Joint.target = samplePoint;
	}

    /**
     * A Vector3 Catmull-Rom spline. Catmull-Rom splines are similar to bezier
     * splines but have the useful property that the generated curve will go
     * through each of the control points.
     *
     * NOTE: The NewCatmullRom() functions are an easier to use alternative to this
     * raw Catmull-Rom implementation.
     *
     * @param previous the point just before the start point or the start point
     *                 itself if no previous point is available
     * @param start generated when elapsedTime == 0
     * @param end generated when elapsedTime >= duration
     * @param next the point just after the end point or the end point itself if no
     *             next point is available
     */
    static Vector2 CatmullRom(Vector2 previous, Vector2 start, Vector2 end, Vector2 next, 
                                float elapsedTime, float duration)
	{
        // References used:
        // p.266 GemsV1
        //
        // tension is often set to 0.5 but you can use any reasonable value:
        // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
        //
        // bias and tension controls:
        // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/
 
        float percentComplete = elapsedTime / duration;
        float percentCompleteSquared = percentComplete * percentComplete;
        float percentCompleteCubed = percentCompleteSquared * percentComplete;
 
        return previous * (-0.5f * percentCompleteCubed +
                                   percentCompleteSquared -
                            0.5f * percentComplete) +
                start   * ( 1.5f * percentCompleteCubed +
                           -2.5f * percentCompleteSquared + 1.0f) +
                end     * (-1.5f * percentCompleteCubed +
                            2.0f * percentCompleteSquared +
                            0.5f * percentComplete) +
                next    * ( 0.5f * percentCompleteCubed -
                            0.5f * percentCompleteSquared);
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected ()
	{
        var pointCount = CurvePoints.Length;

		if (pointCount < 2)
			return;

		Handles.color = CurveColor;
        for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
        {
            var p0 = CurvePoints[i];
            var p1 = CurvePoints[j];
			Handles.DrawLine (p0, p1);
        }
    }
#endif
}
