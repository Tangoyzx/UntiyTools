using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Image))]
    public class SphereImage : BaseMeshEffect
    {
		public int CornerNum = 2;
        protected SphereImage()
        { }

		public override void ModifyMesh (VertexHelper vh) {
			if (!IsActive()) return;
			
			if (vh.currentVertCount != 4) return;

            UIVertex vertice = new UIVertex();
            vh.PopulateUIVertex(ref vertice, 0);
            Vector2 lbPos = vertice.position;
			Vector2 lbUV = vertice.uv0;
            vh.PopulateUIVertex(ref vertice, 2);
            Vector2 rtPos = vertice.position;
			Vector2 rtUV = vertice.uv0;


			Vector2 center = (lbPos + rtPos) * 0.5f;
			Vector2 halfSize = (rtPos - lbPos) * 0.5f;
			var vertexList = new UIVertex[CornerNum + CornerNum + 1];
			var newList = new List<UIVertex>();
			

			var centerVertex = new UIVertex();
			centerVertex.position = new Vector3(center.x, center.y, 0);
			centerVertex.uv0 = (lbUV + rtUV) * 0.5f;
			centerVertex.color = Color.white;
			vertexList[0] = centerVertex;

			var anglePer = Mathf.PI / CornerNum;

			for(var i = 0; i < CornerNum + CornerNum; i ++) {
				var angle = anglePer * i;
				var posX = Mathf.Cos(angle);
				var posY = Mathf.Sin(angle);

				var pixelPosX = posX * halfSize.x;
				var pixelPosY = posY * halfSize.y;

				var uvU = Mathf.Lerp(lbUV.x, rtUV.x, posX * 0.5f + 0.5f);
				var uvV = Mathf.Lerp(lbUV.y, rtUV.y, posY * 0.5f + 0.5f);

				var newVertex = new UIVertex();
				newVertex.position = new Vector3(pixelPosX, pixelPosY, 0);
				newVertex.uv0 = new Vector2(uvU, uvV);
				newVertex.color = Color.white;

				vertexList[i + 1] = newVertex;
			}

			for(var i = 0; i < CornerNum + CornerNum - 1; i++) {
				newList.Add(vertexList[0]);
				newList.Add(vertexList[i + 1]);
				newList.Add(vertexList[i + 2]);
			}

			newList.Add(vertexList[0]);
			newList.Add(vertexList[CornerNum + CornerNum]);
			newList.Add(vertexList[1]);

			vh.Clear();
			vh.AddUIVertexTriangleStream(newList);
		}
	}
}