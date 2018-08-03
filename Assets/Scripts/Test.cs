using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	[SerializeField] private LoopScrollView m_HorLoopScrollView;
	[SerializeField] private LoopScrollView m_VerLoopScrollView;

	void Start()
	{
		m_HorLoopScrollView.ScrollEnd += OnHorLoopScrollViewEnd;
		m_VerLoopScrollView.ScrollEnd += OnVerLoopScrollViewEnd;

		m_HorLoopScrollView.Init(0);
		m_VerLoopScrollView.Init(1);
	}

	private void OnHorLoopScrollViewEnd(object sender,EventArgs e)
	{
		Debug.Log("Horizontal Loop ScrollView Scroll End");
	}

	private void OnVerLoopScrollViewEnd(object sender, EventArgs e)
	{
		Debug.Log("Vertical Loop ScrollView Scroll End");
	}
}
