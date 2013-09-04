using UnityEngine;
using System.Collections;

public class InputSetupData
{
	public KeyCode up;
	public KeyCode down;
	public KeyCode left;
	public KeyCode right;
	public KeyCode button1;
	public KeyCode button2;
	public KeyCode button3;
	public KeyCode button4;
	public KeyCode button5;
	public KeyCode button6;
	public KeyCode button7;
	public KeyCode coin;
	public KeyCode start;
	
	public InputSetupData()
	{
		MapButtons(1);
	}
	public InputSetupData(int setNum)
	{
		MapButtons(setNum);	
	}
	
	private void MapButtons(int setNum)
	{
		if(setNum > 2 || setNum < 1)
			setNum = 1;
		
		switch(setNum)
		{
		case 1:
			up = KeyCode.W;
			down = KeyCode.S;
			left = KeyCode.A;
			right = KeyCode.D;
			button1 = KeyCode.U;
			button2 = KeyCode.I;
			button3 = KeyCode.O;
			button4 = KeyCode.J;
			button5 = KeyCode.K;
			button6 = KeyCode.L;
			button7 = KeyCode.Space;
			coin = KeyCode.Backslash;
			start = KeyCode.Return;
			break;
		case 2:
			up = KeyCode.UpArrow;
			down = KeyCode.DownArrow;
			left = KeyCode.LeftArrow;
			right = KeyCode.RightArrow;
			button1 = KeyCode.Keypad4;
			button2 = KeyCode.Keypad5;
			button3 = KeyCode.Keypad5;
			button4 = KeyCode.Keypad1;
			button5 = KeyCode.Keypad2;
			button6 = KeyCode.Keypad3;
			button7 = KeyCode.Keypad0;
			coin = KeyCode.KeypadPlus;
			start = KeyCode.KeypadEnter;
			break;
		default:
			break;
		}
	}
}
