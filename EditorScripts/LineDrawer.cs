using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
	[SerializeField]
	private Material lineMaterial;

	private List<Vector3> points = new List<Vector3>();
    private List<Color> pointColors = new List<Color>();

    private readonly Color gridColor = new Color(0.0f, 0.0f, 0.0f, 0.25f);

    void OnPostRender()
	{
		if (!GlobalData.playMode || GlobalData.debugMode)
		{
			lineMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Color(gridColor);
			for (float x = -15.5f; x < 15.5f; x += 1.0f)
			{
				GL.Vertex3(x, -100.0f, -20.0f);
				GL.Vertex3(x, 100.0f, -20.0f);
			}
			for (float y = -10.5f; y < 10.5f; y += 1.0f)
			{
				GL.Vertex3(-100.0f, y, -20.0f);
				GL.Vertex3(100.0f, y, -20.0f);
			}
			GL.End();

			if (points != null)
			{
				lineMaterial.SetPass(0);
				GL.Begin(GL.LINES);
                for (int i = 0; i < points.Count; i++)
                {
                    GL.Color(pointColors[i]);
                    GL.Vertex(points[i]);
                }
				GL.End();
			}
		}
	}

    public void AddPoint(Vector3 position, Color color)
    {
        points.Add(position);
        pointColors.Add(color);
    }

    public void Clear()
    {
        points.Clear();
        pointColors.Clear();
    }
}