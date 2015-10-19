using UnityEngine;

[ExecuteInEditMode]

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public abstract class DynamicMesh : MonoBehaviour
{
	
	#region Protected Abstract Properties
	
	protected abstract Vector3[] vertices { get; }
	
	
	protected abstract Vector2[] uv { get; }
	
	
	protected abstract int[] triangles { get; }
	
	#endregion
	
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	#region Protected Properties
	
	protected MeshFilter meshFilter
	{
		get
		{
			if(this._meshFilter == null)
			{
				this._meshFilter = gameObject.GetComponent<MeshFilter>();
			}
			return this._meshFilter;
		}
	}
	
	
	protected Mesh mesh
	{
		get
		{
			if(_mesh != null)
			{
				return _mesh;
			}
			else
			{
				
				if(meshFilter.sharedMesh == null)
				{
					Mesh newMesh = new Mesh();
					_mesh = meshFilter.sharedMesh = newMesh;
				}
				else
				{
					_mesh = meshFilter.sharedMesh;
				}
				return _mesh;
			}
		}
	}
	
	#endregion
	
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	#region Protected Methods
	
	protected virtual void OnEnable()
	{
		ReCalculateMesh(true);
	}
	
	#endregion
	
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	#region Private Methods
	
	private void ReCalculateMesh(bool allAttributes)
	{
		if(allAttributes)
		{
			if(mesh == null)
			{
				Debug.LogError("Could not access or create a mesh", this);
				return;
			}
			mesh.Clear();
		}
		mesh.vertices = vertices;
		
		if(allAttributes)
		{
			mesh.uv = uv;
			mesh.triangles = triangles;
		}
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	
	
	#endregion
	
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	
	#region Private Members
	
	private MeshFilter _meshFilter = null;
	private Mesh _mesh = null;
	
	#endregion
}