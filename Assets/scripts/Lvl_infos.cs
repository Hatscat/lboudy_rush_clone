using UnityEngine;
using System.Collections;

public class Lvl_infos : MonoBehaviour {

	public string lvlName;
	public byte cubesNbPerColor, stepsBetweenSpawns,
				leftArrowsNb, rightArrowsNb, upArrowsNb, downArrowsNb,
				leftConvsNb, rightConvsNb, upConvsNb, downConvsNb,
				splitsNb, stopsNb;
	public Texture2D[] levelDesign;
}
