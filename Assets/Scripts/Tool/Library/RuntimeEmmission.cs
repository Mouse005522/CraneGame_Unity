using UnityEngine;

public class RuntimeEmmission : MonoBehaviour {

	[SerializeField]
	MeshRenderer meshRender;

	[SerializeField][Range(0,2)]
	float scale = 1f;

	// Use this for initialization
	void Start () 
	{
		if (meshRender == null) 
		{
			meshRender = this.GetComponent<MeshRenderer> ();
		}

		if (meshRender != null) 
		{
			Material[] materials = meshRender.materials;

			for (int i = 0; i < materials.Length; i++) 
			{
				if (materials [i] != null) 
				{
					SetMateralEmmission (materials [i]);
				}
				else
				{
					Debug.LogError ($"{this.gameObject.name} has temp mat , index -> {i}");
				}
			}
		}
		else
		{
			Debug.LogError ($"{this.gameObject.name} can't get meshRender");
		}
	}

	#if UNITY_EDITOR

	void Update ()
	{
		Material[] materials = meshRender.materials;

		for (int i = 0; i < materials.Length; i++) 
		{
			if (materials [i] != null) 
			{
				SetMateralEmmission (materials [i]);
			}
		}
	}

	#endif

	void SetMateralEmmission (Material mat)
	{
		Color color = mat.GetColor ("_Color");

		color = new Color (color.r * scale, color.g * scale, color.b * scale, color.a);

		mat.EnableKeyword ("_EMISSION");
		mat.SetColor ("_EmissionColor", color);
	}
}
