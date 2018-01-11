using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct Star {
	public float ra;
	public float de;
	public float ma;
	public string spectralType;
	public string name;
	public int id;

	public override string ToString() {
		return "star data- ra:" + ra + " de: " + de + " ma: " + ma;
	}
}

/// <summary>
/// Starchart script by Aaron "Pyredrid" B-D
/// 
/// Uses Yale Bright Star ASCII Catalogue (http://tdc-www.harvard.edu/catalogs/bsc5.html)
/// </summary>
public class StarChart : MonoBehaviour {
	public float starPlacementDistance = 100.0f;
	public float magnitudeLimit = 4.0f;
	public TextAsset starChartCatalog;
	public GameObject starPrefab;

	private List<Star> stars = new List<Star>();

	[ContextMenu("Chart out stars")]
	void Place() {
		ParseText(starChartCatalog.text);
		foreach(Star star in stars) {
			PlaceStar(star);
		}
		stars = new List<Star>();
		Debug.Log("Done placing stars!  " + stars.Count + " stars mapped");
	}

	private void ParseText(string text) {
		string[] lines = text.Split('\n');
		for(int i = 0; i < lines.Length; i++) {
			Star? potentialStar = ParseLine(lines[i]);
			if(potentialStar.HasValue) {
				Star newStar = potentialStar.Value;
				if(newStar.ma < magnitudeLimit) {
					stars.Add(newStar);
				}
			}
		}
		Debug.Log(stars.Count + " usable stars found in catalog");
	}


	private Star? ParseLine(string line) {
		//Create star from data, if failure, return null
		Star newstar = new Star();
		try {
			//Right Ascension
			string h_ra_str = line.Substring(60, 2);
			string m_ra_str = line.Substring(62, 2);
			string s_ra_str = line.Substring(64, 2);

			float h_ra = float.Parse(h_ra_str);
			float m_ra = float.Parse(m_ra_str);
			float s_ra = float.Parse(s_ra_str);
			newstar.ra = RAToRadians(h_ra, m_ra, s_ra);

			//Declination
			string d_de_str = line.Substring(68, 3);
			string m_de_str = line.Substring(71, 2);
			string s_de_str = line.Substring(73, 2);

			float d_de = float.Parse(d_de_str);
			float m_de = float.Parse(m_de_str);
			float s_de = float.Parse(s_de_str);
			newstar.de = DEToRadians(d_de, m_de, s_de);

			//Apparent Magnitude
			string a_ma_str = line.Substring(103, 4);
			float a_ma = float.Parse(a_ma_str);
			newstar.ma = a_ma;

			//Spectral Type
			string st = line.Substring(128, 20);
			newstar.spectralType = st;

			//Star Name
			string n = line.Substring(5, 9);
			if(n.Equals("         ")) {
				n = "NO_NAME";
			}

			//Include the index for primarily debug reasons
			n = line.Substring(0, 4).Replace(' ', '0') + " " + n;
			newstar.name = n;
		} catch {
			return null;
		}
		return newstar;
	}

	private float RAToRadians(float h_ra, float m_ra, float s_ra) {
		float hours = ( h_ra + ( m_ra + s_ra / 60.0f ) / 60.0f );
		float value = hours / 24.0f * 2.0f * Mathf.PI;
		return value;
	}

	private float DEToRadians(float h_ra, float m_ra, float s_ra) {
		float value = ( h_ra + m_ra / 60.0f ) / 360.0f * 2.0f * Mathf.PI;
		return value;
	}

	private void PlaceStar(Star star) {
		Vector3 position = transform.position;
		Quaternion rotation = Quaternion.Euler(star.de * Mathf.Rad2Deg, star.ra * Mathf.Rad2Deg, 0.0f * Mathf.Rad2Deg);
		Vector3 direction = rotation * Vector3.forward;
		Ray ray = new Ray(position, direction);

		GameObject starGO = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(starPrefab);
		starGO.transform.SetParent(transform);
		starGO.transform.localPosition = ray.GetPoint(starPlacementDistance);
		starGO.transform.localRotation = rotation;

		starGO.transform.parent = transform;
		starGO.name = star.name;
		float scalefactor = 1.0f / star.ma;
		starGO.transform.localScale *= scalefactor;
	}
}