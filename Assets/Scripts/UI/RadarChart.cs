using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class RadarChart : BaseMeshEffect {
	private int _cornerNum;
	private float[] _cornersData;
	private Color _color;

	public static RadarChart Get(GameObject go)
	{
		var radarChart = go.GetComponent<RadarChart>();
		if (radarChart != null)
			return radarChart;
		
		return go.AddComponent<RadarChart>();
	}

	public void SetData(int cornerNum, float[] cornerDatas)
	{
		Debug.Assert(cornerNum >= 3);
		Debug.Assert(cornerDatas.Length == cornerNum);

		_cornerNum = cornerNum;
		_cornersData = cornerDatas;

		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}

	public void SetColor(Color color)
	{
		this._color = color;
	}

	public override void ModifyMesh (VertexHelper vh)
	{
		if (!IsActive()) return;

		if (_cornersData == null) return;

		var vertexList = new List<UIVertex>();
		vh.GetUIVertexStream(vertexList);

		var maxU = float.MinValue;
		var minU = float.MaxValue;
		var maxV = float.MinValue;
		var minV = float.MaxValue;

		var minLen = float.MaxValue;
		for(int i = 0; i < vertexList.Count; i++) 
		{
			var vertex = vertexList[i];
			var vertexPos = vertex.position;
			if (Mathf.Abs(vertexPos.x) < minLen) 
				minLen = Mathf.Abs(vertexPos.x);

			if (Mathf.Abs(vertexPos.y) < minLen) 
				minLen = Mathf.Abs(vertexPos.y);

			maxU = Mathf.Max(vertex.uv0.x, maxU);
			maxV = Mathf.Max(vertex.uv0.y, maxV);
			minU = Mathf.Min(vertex.uv0.x, minU);
			minV = Mathf.Min(vertex.uv0.y, minV);
		}

		var centerV = (minV + maxV) * 0.5f;

		var newList = new List<UIVertex>();

		var circlePer = (Mathf.PI + Mathf.PI) / _cornerNum;
		var newVertexList = new List<Vector3>();

		newVertexList.Add(Vector3.zero);

		for(int i = 0; i < _cornerNum; i++)
		{
			var angle = circlePer * i;
			newVertexList.Add(new Vector3(Mathf.Sin(angle) * minLen * _cornersData[i], Mathf.Cos(angle) * minLen* _cornersData[i], 0));
		}

		for(int i = 0; i < _cornerNum - 1; i++)
		{
			newList.Add(GetVertex(newVertexList[0], new Vector2(minU, centerV), this._color));
			newList.Add(GetVertex(newVertexList[i + 1], new Vector2(maxU, centerV), this._color));
			newList.Add(GetVertex(newVertexList[i + 2], new Vector2(maxU, centerV), this._color));
		}

		newList.Add(GetVertex(newVertexList[0], new Vector2(minU, centerV), this._color));
		newList.Add(GetVertex(newVertexList[_cornerNum], new Vector2(maxU, centerV), this._color));
		newList.Add(GetVertex(newVertexList[1], new Vector2(maxU, centerV), this._color));

		vh.Clear();
		vh.AddUIVertexTriangleStream(newList);
	}

	private static UIVertex GetVertex(Vector3 pos, Vector2 uv, Color color)
	{
		var newVertex = new UIVertex();
		newVertex.position = pos;
		newVertex.uv0 = uv;
		newVertex.color = color;
		return newVertex;
	}
}
