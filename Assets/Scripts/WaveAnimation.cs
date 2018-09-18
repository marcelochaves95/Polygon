using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaveAnimation : MonoBehaviour {

	GameObject[] siblings = new GameObject[0];
	int index = 0;
	float offset = 0.00f;
	float slideMin = -0.2f;
	float slideMax = 0.2f;
	float slideSpeed = 0.5f;
	float slideSharpness = 1.00f;
	float scaleMin = 1.00f;
	float scaleMax = 0.40f;
	float scaleSpeed = 0.50f;
	float scaleSharpness = 0.50f;

	float fadeSpeed = 1.0f;

	Vector3 baseScroll = new Vector3(0.1f, 0, 0.3547f);
	float baseRotation = 0.00f;
	Vector3 baseScale = new Vector3 (10.0f, 10, 10.0f);

	Material theMaterial;
	float slide = 0.00f;
	float slideInertia = 0.00f;
	float scale = 0.00f;
	float scaleInertia = 0.00f;
	Vector3 basePos;
	Vector3 texScale;
	float lastSlide = 0.00f;
	float fade = 1.00f;
	Color color;
	Color fadeColor;
	WaveAnimation original;

	void Start ()
	{
		CheckHWSupport();
		
		Array waves;
		waves = this.GetComponents<WaveAnimation>();
		if(waves.Length == 1 && original == null)
		{
			original = this;	
		}
		
		foreach(GameObject s  in siblings)
		{
			AddCopy(s, original, false);	
		}
		if(waves.Length < this.GetComponent<Renderer>().materials.Length)
		{
			AddCopy(gameObject, original, true);
		}
		theMaterial = this.GetComponent<Renderer>().materials[index];
		color = theMaterial.GetColor("_Color");
		fadeColor = color;
		fadeColor.a = 0;
		texScale = theMaterial.GetTextureScale("_MainTex");	
	}

	void CheckHWSupport()
	{
		var supported = this.GetComponent<Renderer>().sharedMaterial.shader.isSupported;
		foreach(GameObject s  in siblings)
			s.GetComponent<Renderer>().enabled = supported;
		this.GetComponent<Renderer>().enabled = supported;
	}


	void Update ()
	{
		CheckHWSupport();
		
		slideInertia = Mathf.Lerp(slideInertia, Mathf.PingPong((Time.time * scaleSpeed) + offset, 1), slideSharpness * Time.deltaTime);
		slide = Mathf.Lerp(slide, slideInertia, slideSharpness * Time.deltaTime);
		theMaterial.SetTextureOffset("_MainTex", new Vector3(index * 0.35f, Mathf.Lerp(slideMin, slideMax, slide) * 2, 0));
		theMaterial.SetTextureOffset("_Cutout", new Vector3(index * 0.79f, Mathf.Lerp(slideMin, slideMax, slide) / 2, 0));
		
		fade = Mathf.Lerp(fade, slide - lastSlide > 0 ? 0 : 1, Time.deltaTime * fadeSpeed);
		lastSlide = slide;
		theMaterial.SetColor("_Color", Color.Lerp(fadeColor, color, fade));
		
		scaleInertia = Mathf.Lerp(scaleInertia, Mathf.PingPong((Time.time * scaleSpeed) + offset, 1), scaleSharpness * Time.deltaTime);
		scale = Mathf.Lerp(scale, scaleInertia, scaleSharpness * Time.deltaTime);
		theMaterial.SetTextureScale("_MainTex", new Vector3(texScale.x, Mathf.Lerp(scaleMin,scaleMax, scale), texScale.z));
		
		basePos += baseScroll * Time.deltaTime;
		var inverseScale = new Vector3 (1 / baseScale.x, 1 / baseScale.y, 1 / baseScale.z);
		var uvMat = Matrix4x4.TRS (basePos, Quaternion.Euler (baseRotation,90,90), inverseScale);
		theMaterial.SetMatrix ("_WavesBaseMatrix", uvMat);
	}


	void AddCopy (GameObject ob, WaveAnimation original, bool copy)
	{
		WaveAnimation newWave = ob.AddComponent<WaveAnimation>();
		newWave.original = original;
		if(copy) newWave.index = index + 1;
		else newWave.index = index;
		newWave.offset = original.offset + (2.00f / (float)this.GetComponent<Renderer>().materials.Length);
		newWave.slideMin = original.slideMin;
		newWave.slideMax = original.slideMax;
		newWave.slideSpeed = original.slideSpeed + UnityEngine.Random.Range(-original.slideSpeed / 5, original.slideSpeed / 5);
		newWave.slideSharpness = original.slideSharpness + UnityEngine.Random.Range(-original.slideSharpness / 5, original.slideSharpness / 5);
		newWave.scaleMin = original.scaleMin;
		newWave.scaleMax = original.scaleMax;
		newWave.scaleSpeed = original.scaleSpeed + UnityEngine.Random.Range(-original.scaleSpeed / 5, original.scaleSpeed / 5);
		newWave.scaleSharpness = original.scaleSharpness + UnityEngine.Random.Range(-original.scaleSharpness / 5, original.scaleSharpness / 5);
		
		newWave.fadeSpeed = original.fadeSpeed;
			
		Vector3 randy = UnityEngine.Random.onUnitSphere; 
		randy.y = 0;
		newWave.baseScroll = randy.normalized * original.baseScroll.magnitude;
		newWave.baseRotation = UnityEngine.Random.Range(0,360);
		newWave.baseScale = original.baseScale * UnityEngine.Random.Range(0.8f, 1.2f);	
	}
}
