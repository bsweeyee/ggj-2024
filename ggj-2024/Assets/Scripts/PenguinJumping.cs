using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinJumping : Pinguin, ILaunch
{
	public void OnLaunch(Ball ball)
	{
		Jump();
	}
	
	void Jump()
	{
		Debug.Log("JUMP!");
		
		
	}
}
