using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinMoving : Pinguin, ILaunch
{

	public direction jumpingDirection;

	public enum direction
	{
		right, left, up, down
	}

	public void OnLaunch(Ball ball)
	{
		Move();
	}
	
	void Move()
	{
		switch (jumpingDirection)
		{
			case direction.right:
                animator.SetTrigger("MoveRight");
                break;
			case direction.left:
                animator.SetTrigger("MoveLeft");
                break;
			case direction.up:
                animator.SetTrigger("MoveUp");
                break;
			case direction.down:
                animator.SetTrigger("MoveDown");
                break;
			default:
				break;
		}
	}
}
