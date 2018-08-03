using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoopScrollView : MonoBehaviour
{
	private enum ScrollType
	{
		Horizontal,
		Vertical,
	}

	[SerializeField] private bool m_IsLoop;
	[SerializeField] private ScrollType m_ScrollType;
	[SerializeField] private float m_CanScrollSpeed;
	[SerializeField] private GameObject m_Content;
	[SerializeField] private int m_ShowItemCount = 3;
	[SerializeField] private float m_ItemWidth;
	[SerializeField] private float m_TweenMoveSpeed = 500;

	[SerializeField] private bool m_IsHighlightShow;
	[SerializeField] private float m_HighlightScale = 1.2f;
	[SerializeField] private float m_TweenScaleDuration = 0.5f;

	private enum DragDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	private RectTransform m_ContentRect;
	private Dictionary<GameObject, int> m_ItemObjIndexs;
	private List<GameObject> m_ItemObjs;

	private bool m_IsCanDrag;
	private bool m_IsDrag;

	private Vector3 m_LastMousePosition;
	private float m_LastMouseDragTime;
	private DragDirection m_DragDirection;
	private int m_CurrentItemIndex = -1;

	private Tween m_MoveTween;
	private Tween m_ScaleTween;


	public event EventHandler ScrollEnd; 
	void Start()
	{
		//Init(0);//Test
	}

	public void Init(int index)
	{
		m_IsCanDrag = true;
		m_IsDrag = false;
		m_ContentRect = m_ContentRect ?? m_Content.GetComponent<RectTransform>();
		m_ItemObjIndexs = m_ItemObjIndexs ?? new Dictionary<GameObject, int>();
		m_ItemObjs = m_ItemObjs ?? new List<GameObject>();
		for(int i = 0; i < m_Content.transform.childCount; i++)
		{
			m_ItemObjIndexs.Add(m_Content.transform.GetChild(i).gameObject, i);
			m_ItemObjs.Add(m_Content.transform.GetChild(i).gameObject);
		}

		if(m_MoveTween != null)
		{
			m_MoveTween.Kill();
		}

		if(m_ScaleTween != null)
		{
			m_ScaleTween.Kill();
		}

		ShowItem(index, true);
	}

	private int GetItemIndex(int index)
	{
		GameObject itemObj = null;
		foreach(var i in m_ItemObjIndexs)
		{
			if(i.Value == index)
			{
				itemObj = i.Key;
			}
		}

		if(itemObj == null)
		{
			return -1;
		}

		for(int i = 0; i < m_ItemObjs.Count; i++)
		{
			if(itemObj == m_ItemObjs[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void ShowItemDir(DragDirection dragDirection)
	{
		int chooseIndex = (dragDirection == DragDirection.Right || dragDirection == DragDirection.Down)
			? m_CurrentItemIndex + 1
			: m_CurrentItemIndex - 1;
		ShowItem(chooseIndex, false);
	}

	public void ShowItem(int index, bool isFindIndex)
	{
		int itemIndex;
		if(m_IsLoop)
		{
			if(m_ItemObjs.Count <= m_ShowItemCount)
			{
				return;
			}

			if(isFindIndex)
			{
				itemIndex = GetItemIndex(index);
			}
			else
			{
				itemIndex = index;
			}
			itemIndex = UpdateItemObj(itemIndex);
		}
		else
		{
			itemIndex = index;
			if(itemIndex >= m_ItemObjIndexs.Count || itemIndex < 0)
			{
				return;
			}
		}
		HighlightShowItem(m_CurrentItemIndex, false);

		if(m_ScrollType == ScrollType.Horizontal)
		{
			MoveContentHorizontal(itemIndex);
		}
		else if(m_ScrollType == ScrollType.Vertical)
		{
			MoveContentVertical(itemIndex);
		}
	}

	private void MoveContentHorizontal(int showItemIndex)
	{
		float rectPosX = -m_ItemObjs[showItemIndex].GetComponent<RectTransform>().anchoredPosition.x;
		float duration = Mathf.Abs(m_ContentRect.anchoredPosition.x - rectPosX) / m_TweenMoveSpeed;
		if(m_MoveTween != null)
		{
			m_MoveTween.Kill();
		}
		m_MoveTween = m_ContentRect.DOAnchorPosX(rectPosX, duration).OnComplete(() =>
		{
			HighlightShowItem(showItemIndex, true);

			if (ScrollEnd != null)
			{
				ScrollEnd(this,EventArgs.Empty);
			}

		}).SetEase(Ease.OutSine);
		m_CurrentItemIndex = showItemIndex;
	}

	private void MoveContentVertical(int showItemIndex)
	{
		float rectPosY = -m_ItemObjs[showItemIndex].GetComponent<RectTransform>().anchoredPosition.y;
		float duration = Mathf.Abs(m_ContentRect.anchoredPosition.y - rectPosY) / m_TweenMoveSpeed;
		if(m_MoveTween != null)
		{
			m_MoveTween.Kill();
		}
		m_MoveTween = m_ContentRect.DOAnchorPosY(rectPosY, duration).OnComplete(() =>
		{
			HighlightShowItem(showItemIndex, true);

			if(ScrollEnd != null)
			{
				ScrollEnd(this, EventArgs.Empty);
			}

		}).SetEase(Ease.OutSine);
		m_CurrentItemIndex = showItemIndex;
	}

	private int UpdateItemObj(int itemIndex)
	{
		int itemIndexTemmp = itemIndex;
		if(itemIndex == 0)
		{
			GameObject item = m_ItemObjs[m_ItemObjs.Count - 1];
			Vector2 item0Pos = m_ItemObjs[0].GetComponent<RectTransform>().anchoredPosition;

			if(m_ScrollType == ScrollType.Horizontal)
			{
				item.GetComponent<RectTransform>().anchoredPosition = new Vector2(item0Pos.x - m_ItemWidth, item0Pos.y);
			}
			else if(m_ScrollType == ScrollType.Vertical)
			{
				item.GetComponent<RectTransform>().anchoredPosition = new Vector2(item0Pos.x, item0Pos.y + m_ItemWidth);
			}
			m_ItemObjs.RemoveAt(m_ItemObjs.Count - 1);
			m_ItemObjs.Insert(0, item);
			itemIndexTemmp = 1;
			m_CurrentItemIndex = itemIndexTemmp + 1;
		}
		if(itemIndex == m_ItemObjs.Count - 1)
		{
			GameObject item = m_ItemObjs[0];
			Vector2 itemEndPos = m_ItemObjs[m_ItemObjs.Count - 1].GetComponent<RectTransform>().anchoredPosition;
			if(m_ScrollType == ScrollType.Horizontal)
			{
				item.GetComponent<RectTransform>().anchoredPosition = new Vector2(itemEndPos.x + m_ItemWidth, itemEndPos.y);
			}
			else if(m_ScrollType == ScrollType.Vertical)
			{
				item.GetComponent<RectTransform>().anchoredPosition = new Vector2(itemEndPos.x, itemEndPos.y - m_ItemWidth);
			}
			m_ItemObjs.RemoveAt(0);
			m_ItemObjs.Add(item);
			itemIndexTemmp = itemIndex - 1;
			m_CurrentItemIndex -= 1;
		}
		return itemIndexTemmp;
	}

	private void HighlightShowItem(int index, bool isShow)
	{
		if(!m_IsHighlightShow)
		{
			return;
		}

		if(index < 0 || index >= m_ItemObjs.Count)
		{
			return;
		}

		GameObject item = m_ItemObjs[index];
		if(isShow)
		{
			m_ScaleTween = item.transform.DOScale(m_HighlightScale, m_TweenScaleDuration);
		}
		else
		{
			if(m_ScaleTween != null)
			{
				m_ScaleTween.Kill();
			}
			item.transform.localScale = Vector3.one;
		}
	}

	public void Drag()
	{
		if(!m_IsCanDrag)
		{
			return;
		}

		if(!m_IsDrag)
		{
			m_LastMousePosition = Input.mousePosition;
			m_LastMouseDragTime = Time.time;
			m_IsDrag = true;
			return;
		}

		Vector3 mousePosDiff = Input.mousePosition - m_LastMousePosition;

		float mousePosDiffDir = m_ScrollType == ScrollType.Horizontal ? mousePosDiff.x : mousePosDiff.y;

		if(Mathf.Abs(mousePosDiffDir) / (Time.time - m_LastMouseDragTime) < m_CanScrollSpeed)
		{
			m_IsDrag = false;

			return;
		}
		m_IsCanDrag = false;
		m_DragDirection = m_ScrollType == ScrollType.Horizontal
			? (mousePosDiffDir > 0 ? DragDirection.Left : DragDirection.Right)
			: (mousePosDiffDir > 0 ? DragDirection.Down : DragDirection.Up);
		ShowItemDir(m_DragDirection);
	}

	public void OnEndDrag()
	{
		m_IsCanDrag = true;
		m_IsDrag = false;
	}

	public void OnClickItem(GameObject clickItem)
	{
		int itemIndex = -1;
		for(int i = 0; i < m_ItemObjs.Count; i++)
		{
			if(clickItem == m_ItemObjs[i])
			{
				itemIndex = i;
			}
		}

		if(itemIndex == -1)
		{
			return;
		}
		ShowItem(itemIndex, false);
	}
}
