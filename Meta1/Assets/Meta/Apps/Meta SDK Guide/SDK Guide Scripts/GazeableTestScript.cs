/************************************************************************************
 *  Copyright © 2014 Meta Company. All Rights Reserved. Use of this software source *
 *  code and binaries requires agreement and compliance with the META Licensed 		*
 *  Application End User License Agreement in the accompanying META EULA.txt file, 	*
 *  which must be included as part of this software for any use. 					*
 ************************************************************************************/

using UnityEngine;
using System.Collections;

namespace Meta.Apps.SDKGuide
{

	public class GazeableTestScript : MonoBehaviour
	{
		public void OnGazeEnter()
		{
			GetComponent<Renderer>().material.color = Color.red;
		}
		public void OnGazeHold()
		{
			GetComponent<Renderer>().material.color = Color.green;
		}
		public void OnGazeDwell()
		{
			GetComponent<Renderer>().material.color = Color.cyan;
		}
		public void OnGazeExit()
		{
			GetComponent<Renderer>().material.color = Color.blue;
		}
	}
}