using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SurfaceCreator : MonoBehaviour {

	private Mesh mesh;
	[Range(1,200)]
	public int resolution = 10;
	
	public float frequency = 1f;
	[Range(1, 8)]
	public int octaves = 1;
	[Range(1f, 4f)]
	public float lacunarity = 2f;
	[Range(0f, 1f)]
	public float persistence = 0.5f;
	[Range(1, 3)]
	public int dimensions = 3;
	
	public NoiseMethodType type;
	public Gradient coloring;
	
	private int currentResolution;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] colors;
	
	private void OnEnable(){
		if (mesh == null){
			mesh = new Mesh();
			mesh.name = "Surface Mesh";
			GetComponent<MeshFilter>().mesh = mesh;
		}
		Refresh();
	}
	
	public void Refresh(){
		if (resolution != currentResolution){
			CreateGrid();
		}
		Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));
		
		NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int v = 0, y = 0; y <= resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, y * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, y * stepSize);
			for (int x = 0; x <= resolution; x++, v++) {
				Vector3 point = Vector3.Lerp(point0, point1, x * stepSize);
				float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				if (type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}
				colors[v] = coloring.Evaluate(sample);
			}
		}
		mesh.colors = colors;
	}
	
	private void CreateGrid(){
		currentResolution = resolution;
		mesh.Clear();
		vertices = new Vector3[(resolution + 1) * (resolution + 1)];
		colors = new Color[vertices.Length];
		normals = new Vector3[vertices.Length];
		Vector2[] uv = new Vector2[vertices.Length];
		float stepSize = 1f / resolution;
		for (int v = 0, y = 0; y<=resolution; y++){
			for (int x =0; x <= resolution; x++, v++){
				vertices[v] = new Vector3(x * stepSize - 0.5f, y * stepSize - 0.5f);
				colors[v] = Color.black;
				normals[v] = Vector3.back;
				uv[v] = new Vector2(x * stepSize, y * stepSize);
			}
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.normals = normals;
		mesh.uv = uv;
		
		int[] triangles  = new int[resolution * resolution * 6];
		for (int t = 0, v = 0, y = 0; y < resolution; y++, v++){
			for (int x = 0; x < resolution; x++, v++, t+= 6){
				triangles [t] = v;
				triangles [t + 1] = v + resolution + 1;
				triangles [t + 2] = v + 1;
				triangles [t + 3] = v + 1;
				triangles [t + 4] = v + resolution + 1;
				triangles [t + 5] = v + resolution + 2;
			}
		}
		mesh.triangles = triangles;
	}

}
