using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CustomTools {
	public class NoiseMaker : EditorWindow {
		[MenuItem("CustomTools/NoiseMaker", false, 1)]
		public static void Open() {
			GetWindow<NoiseMaker>();
		}

		private string octaveStr = "5";
		private string frequencyStr = "32";

		private string sizeStr = "256";

		private string warningStr = null;

		private int[] randomList;
		private Vector2[] circleList;

		void OnGUI() {

			GUILayout.Width(500);
            GUILayout.Height(800);
			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Size: ");
			sizeStr = GUILayout.TextField(sizeStr);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Octave: ");
			octaveStr = GUILayout.TextField(octaveStr);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Frequency: ");
			frequencyStr = GUILayout.TextField(frequencyStr);
			GUILayout.EndHorizontal();

			if (GUILayout.Button("创建")) {
				CreateNoise();
			}

			if (warningStr != null) {
				GUILayout.Label(warningStr);
			}
		}

		private void CreateNoise() {
			int size, octave, frequency;

			if (int.TryParse(sizeStr, out size) && int.TryParse(octaveStr, out octave) && int.TryParse(frequencyStr, out frequency)) {
			} else {
				warningStr = "参数整形转换错误";
				return;
			}

			size = Mathf.NextPowerOfTwo(size);
			sizeStr = size.ToString();

			frequency = Mathf.NextPowerOfTwo(frequency);
			frequencyStr = frequency.ToString();

			octave = Mathf.RoundToInt(Mathf.Min(octave, Mathf.Log(frequency, 2)));
			octaveStr = octave.ToString();

			randomList = CreateRandomList(size);
			circleList = CreateCircleList(size);

			var color = new Color[size * size];

			for(var i = 0; i < size; i++) {
				for(var j = 0; j < size; j++) {
					var v = fbm((float)j / frequency, (float)i / frequency, (size / frequency), 5);
					// v = Mathf.Abs(v);
					color[i * size + j] = new Color(v, v, v, 1);
				}
			}

			var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
			tex.SetPixels(color);

			var bytes = tex.EncodeToPNG();

			File.WriteAllBytes(Application.dataPath + "/Editor/Noise/output.png", bytes);

			randomList = null;
			circleList = null;
		}

		private float surflet(float x, float y, int gridX, int gridY, int per) {
			var distX = Mathf.Abs(x - gridX);
			var distY = Mathf.Abs(y - gridY);

			var polyX = 1 - 6 * Mathf.Pow(distX, 5) + 15 * Mathf.Pow(distX, 4) - 10 * Mathf.Pow(distX, 3);
			var polyY = 1 - 6 * Mathf.Pow(distY, 5) + 15 * Mathf.Pow(distY, 4) - 10 * Mathf.Pow(distY, 3);

			var hashed = randomList[randomList[gridX % per] + gridY % per];
			var grad = (x - gridX) * circleList[hashed].x + (y - gridY) * circleList[hashed].y;
			
			return polyX * polyY * grad;
		}
		private float noise(float x, float y, int per) {
			var intX = (int)x;
			var intY = (int)y;

			return surflet(x, y, intX+0, intY+0, per) + 
				   surflet(x, y, intX+0, intY+1, per) + 
				   surflet(x, y, intX+1, intY+0, per) + 
				   surflet(x, y, intX+1, intY+1, per);
		}

		private float fbm(float x, float y, int per, int octs) {
			var val = 0.0f;
			for (var i = 0; i < octs; i++) {
				val += Mathf.Pow(0.5f, i) * noise(x * Mathf.Pow(2, i), y * Mathf.Pow(2, i), (int)(per * Mathf.Pow(2, i)));
			}
			val = val * 0.5f + 0.5f;
			return val;
		}

		private float CalPerlinNoise(float x, float y, int gridX, int gridY, int per) {
			float distX = Mathf.Abs(x - gridX);
			float distY = Mathf.Abs(y - gridY);

			var gIndex = randomList[randomList[gridX % per] + gridY % per];
			var g = circleList[gIndex];

			var polyX = 1 - 6 * Mathf.Pow(distX, 5) + 15 * Mathf.Pow(distX, 4) - 10 * Mathf.Pow(distX, 3);
			var polyY = 1 - 6 * Mathf.Pow(distY, 5) + 15 * Mathf.Pow(distY, 4) - 10 * Mathf.Pow(distY, 3);

			var grad = (x - gridX) * g.x + (y - gridY) * g.y;

			return polyX * polyY * grad;
		}

		private float PerlinNoise(float x, float y, int per) {
			int gridX = (int)x;
			int gridY = (int)y;

			return CalPerlinNoise(x, y, gridX, gridY, per) + CalPerlinNoise(x, y, gridX + 1, gridY, per) + CalPerlinNoise(x, y, gridX, gridY + 1, per) + CalPerlinNoise(x, y, gridX + 1, gridY + 1, per);
		}


		private int[] CreateRandomList(int size) {
			size = 256;
			var res = new int[size];
			for(var i = 0; i < size; i++) {
				res[i] = i;
			}

			for(var i = 0; i < size; i++) {
				var a = Random.Range(0, size - 1);
				var b = Random.Range(a + 1, size);
				var t = res[a];
				res[a] = res[b];
				res[b] = t;
			}

			var rres = new int[size + size];
			for(var i = 0; i < size; i++) {
				rres[i] = res[i];
				rres[i + size] = res[i];
			}

			return rres;
		}

		private Vector2[] CreateCircleList(int size) {
			size = 256;
			var res = new Vector2[size];
			for(var i = 0; i < size; i++) {
				var angle = 2.0f * Mathf.PI * i / size;
				res[i] = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
			}

			return res;
		}
	}
}
