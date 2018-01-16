using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Image))]
    public class PolygonImage : BaseMeshEffect
    {
        protected PolygonImage()
        { }

        public override void ModifyMesh(VertexHelper vh)
        {
            Image image = GetComponent<Image>();
            if(image.type != Image.Type.Simple)
            {
                return;
            }
            Sprite sprite = image.overrideSprite;
            if(sprite == null || sprite.triangles.Length == 6)
            {
                // only 2 triangles
                return;
            }

            if (vh.currentVertCount != 4)
            {
                return;
            }
            UIVertex vertice = new UIVertex();
            vh.PopulateUIVertex(ref vertice, 0);
            Vector2 lb = vertice.position;
            vh.PopulateUIVertex(ref vertice, 2);
            Vector2 rt = vertice.position;
            
            int len = sprite.vertices.Length;
            var vertices = new List<UIVertex>(len);
            
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            for (int i = 0; i < len; i++)
            {
                minX = Mathf.Min(minX, sprite.vertices[i].x);
                maxX = Mathf.Max(maxX, sprite.vertices[i].x);
                minY = Mathf.Min(minY, sprite.vertices[i].y);
                maxY = Mathf.Max(maxY, sprite.vertices[i].y);
            }

            Vector2 invExtend = new Vector2(1.0f / (maxX - minX), 1.0f / (maxY - minY));
            for (int i = 0; i < len; i++)
            {
                vertice = new UIVertex();
                
                float x = (sprite.vertices[i].x - minX) * invExtend.x;
                float y = (sprite.vertices[i].y - minY) * invExtend.y;
                
                vertice.position = new Vector2(Mathf.Lerp(lb.x, rt.x, x), Mathf.Lerp(lb.y, rt.y, y));
                vertice.color = image.color;
                vertice.uv0 = sprite.uv[i];
                vertices.Add(vertice);
            }

            len = sprite.triangles.Length;

            var triangles = new List<int>(len);
            for (int i = 0; i < len; i++)
            {
                triangles.Add(sprite.triangles[i]);
            }

            vh.Clear();
            vh.AddUIVertexStream(vertices, triangles);
        }
    }
}