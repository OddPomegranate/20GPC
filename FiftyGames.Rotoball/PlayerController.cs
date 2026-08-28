using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FiftyGames.Rotoball;

internal class PlayerController
{
	private Player m_Player;

	private Vector2 m_Position;

	private Color m_Colour;

	private float m_Scale;

	private bool m_Alive;

	private Texture2D backingSprite;

	private Texture2D forgroundSprite;

	private int currentSelectionIndex;

	private int teamIndex;

	private int selfIndex;

	private bool switchLock;

	private bool switchLockAbs;

	private PlayerManager pManager;

	public PlayerController(Player player, PlayerManager inPlayerManager, int initialSelection, int inSelfIndex)
	{
		m_Player = player;
		m_Scale = 1f;
		pManager = inPlayerManager;
		selfIndex = initialSelection;
		currentSelectionIndex = initialSelection;
		m_Colour = pManager.GetPlayerColor(player);
	}

	public void playerSwitchingHandlerOld(List<Pawn> pList)
	{
		bool flag = true;
		if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed && !switchLock)
		{
			switchLock = true;
			int num = currentSelectionIndex;
			pList[num].detatchFromPawn();
			num -= ((teamIndex == 2) ? 3 : 0);
			while (flag)
			{
				num++;
				if (num > 2)
				{
					num = 0;
				}
				flag = pList[num + ((teamIndex == 2) ? 3 : 0)].attatchToPawn(selfIndex + 1);
				if (num == 3)
				{
					throw new Exception("WHAT?");
				}
			}
			currentSelectionIndex = num + ((teamIndex == 2) ? 3 : 0);
		}
		if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Released && switchLock)
		{
			switchLock = false;
		}
	}

	public void playerSwitchingHandlerNew(List<Pawn> pList)
	{
		bool flag = true;
		if ((m_Player.GamePadManager.GamePadStateCurrent.Buttons.LeftShoulder == ButtonState.Pressed || m_Player.GamePadManager.GamePadStateCurrent.Buttons.RightShoulder == ButtonState.Pressed) && !switchLock)
		{
			int num = 0;
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.LeftShoulder == ButtonState.Pressed)
			{
				num = -1;
			}
			else if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.RightShoulder == ButtonState.Pressed)
			{
				num = 1;
			}
			switchLock = true;
			int num2 = currentSelectionIndex;
			pList[num2].detatchFromPawn();
			num2 -= ((teamIndex == 2) ? 3 : 0);
			while (flag)
			{
				num2 += num;
				if (num2 > 2)
				{
					num2 = 0;
				}
				if (num2 < 0)
				{
					num2 = 2;
				}
				flag = pList[num2 + ((teamIndex == 2) ? 3 : 0)].attatchToPawn(selfIndex + 1);
				if (num2 < 0 && num2 > 3)
				{
					throw new Exception("WHAT?");
				}
			}
			currentSelectionIndex = num2 + ((teamIndex == 2) ? 3 : 0);
		}
		if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.LeftShoulder == ButtonState.Released && m_Player.GamePadManager.GamePadStateCurrent.Buttons.RightShoulder == ButtonState.Released && switchLock)
		{
			switchLock = false;
		}
	}

	public void playerSwitchingHandlerAbsolute(List<Pawn> pList)
	{
		if ((m_Player.GamePadManager.GamePadStateCurrent.Buttons.X == ButtonState.Pressed || m_Player.GamePadManager.GamePadStateCurrent.Buttons.Y == ButtonState.Pressed || m_Player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed) && !switchLockAbs)
		{
			int num = 0;
			if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.X == ButtonState.Pressed)
			{
				num = 0;
			}
			else if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.Y == ButtonState.Pressed)
			{
				num = 1;
			}
			else if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Pressed)
			{
				num = 2;
			}
			switchLockAbs = true;
			int num2 = currentSelectionIndex;
			pList[num2].detatchFromPawn();
			num2 -= ((teamIndex == 2) ? 3 : 0);
			if (pList[num + ((teamIndex == 2) ? 3 : 0)].attatchToPawn(selfIndex + 1))
			{
				pList[num2 + ((teamIndex == 2) ? 3 : 0)].attatchToPawn(selfIndex + 1);
			}
			else
			{
				num2 = num;
				pList[num2 + ((teamIndex == 2) ? 3 : 0)].attatchToPawn(selfIndex + 1);
			}
			currentSelectionIndex = num2 + ((teamIndex == 2) ? 3 : 0);
		}
		if (m_Player.GamePadManager.GamePadStateCurrent.Buttons.X == ButtonState.Released && m_Player.GamePadManager.GamePadStateCurrent.Buttons.Y == ButtonState.Released && m_Player.GamePadManager.GamePadStateCurrent.Buttons.B == ButtonState.Released && switchLockAbs)
		{
			switchLockAbs = false;
		}
	}

	public void Update(List<Pawn> pList)
	{
		playerSwitchingHandlerNew(pList);
		playerSwitchingHandlerAbsolute(pList);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
	}

	public void setTeamIndex(int index)
	{
		teamIndex = index;
	}

	public Player getPlayerReference()
	{
		return m_Player;
	}

	public void setCurrentSelection(int index)
	{
		currentSelectionIndex = index;
	}

	public Color getColor()
	{
		return m_Colour;
	}
}
