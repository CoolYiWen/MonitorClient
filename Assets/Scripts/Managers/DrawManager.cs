﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawManager : MonoBehaviour
{
	public float Height = 0.1f;
	public int CirclePointCount = 10;
	public int RequestFPS = 5;
	public LineRenderer m_RefLineRenderer;
	public LineRenderer m_MapLineRenderer;

	private int m_iUpdateTime = 0;
	private GetRefLineApi m_GetRefLineApi;
	private Dictionary<int, RefLineData> m_RefLineDict;


	void Awake()
	{
		this.m_GetRefLineApi = new GetRefLineApi ();
		this.m_GetRefLineApi.AddCallback (this.SetRefLineData);

	}

	// Use this for initialization
	void Start ()
	{
		this.m_RefLineDict = new Dictionary<int, RefLineData> ();
		this.m_RefLineRenderer.SetWidth (0.2f, 0.2f);

		SignalManager.Instance.AddHandler (SignalID.GameView_ControlChanged, this.DrawCallback);
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.m_iUpdateTime += 1;

		if(! GlobalManager.Instance.IsGameRunning)
		{
			return;
		}

		if (this.m_iUpdateTime >= this.RequestFPS)
		{
			if (this.m_GetRefLineApi.IsIdle())
			{
				StartCoroutine (this.m_GetRefLineApi.Request(GlobalManager.Instance.GetInstanceID()));
				this.m_iUpdateTime = 0;
			}
		}



	}

	private void DrawCallback(object oSender, object oParam)
	{
		Draw ();
	}

	private void Draw()
	{
		int iShapeID = ShipManager.Instance.ControlShipID;
		if(! this.m_RefLineDict.ContainsKey(iShapeID))
		{
			return;
		}

		var oRefLineData = this.m_RefLineDict [iShapeID];

		switch(oRefLineData.flag)
		{
		case 0:
			DrawNone ();
			break;
		case 1:
			DrawLine (oRefLineData);
			break;
		case 2:
			DrawCircle (oRefLineData);
			break;
		default:
			break;
		}
	}

	/// <summary>
	/// 不画东西
	/// </summary>
	private void DrawNone()
	{
		this.m_RefLineRenderer.SetVertexCount (0);
	}

	/// <summary>
	/// 画线
	/// </summary>
	/// <param name="oRefLineData">O reference line data.</param>
	private void DrawLine(RefLineData oRefLineData)
	{
		this.m_RefLineRenderer.SetVertexCount (2);
		this.m_RefLineRenderer.SetPosition (0, new Vector3 (0, Height, (float)oRefLineData.posY));
		this.m_RefLineRenderer.SetPosition (1, new Vector3 (GlobalManager.Instance.MapSize.x, Height, (float)oRefLineData.posY));
	}

	/// <summary>
	/// 画圆
	/// </summary>
	/// <param name="oRefLineData">O reference line data.</param>
	private void DrawCircle(RefLineData oRefLineData)
	{
		this.m_RefLineRenderer.SetVertexCount (CirclePointCount + 1);
		var pointsList = CalculatePoints (oRefLineData);
		for(int i=0; i < CirclePointCount; i++)
		{
			this.m_RefLineRenderer.SetPosition (i, pointsList [i]);
		}
		if (pointsList.Count > 0)
		{
			this.m_RefLineRenderer.SetPosition (CirclePointCount, pointsList [0]);
		}
	}

	private List<Vector3> CalculatePoints(RefLineData oRefLineData)
	{
		float angle = 360f / CirclePointCount;
		var pointsList = new List<Vector3> ();
		Vector3 v3CenterPoint = new Vector3 ((float)oRefLineData.posX, Height, (float)oRefLineData.posY);
		transform.position = v3CenterPoint;
		Vector3 v = transform.position + transform.forward * (float)oRefLineData.radius;
		pointsList.Add (v);
		Quaternion r = transform.rotation;
		for(int i=1; i < CirclePointCount; i++)
		{
			Quaternion q = Quaternion.Euler (r.eulerAngles.x, r.eulerAngles.y - (angle * i), r.eulerAngles.z);
			v = transform.position + (q * Vector3.forward) * (float)oRefLineData.radius;
			pointsList.Add (v);
		}

		return pointsList;
	}

	/// <summary>
	/// 设置参考线
	/// </summary>
	/// <param name="oSender">sender.</param>
	/// <param name="oData">RefLine Data</param>
	public void SetRefLineData(object oSender, object oData)
	{
		if(!GlobalManager.Instance.IsGameRunning)
		{
			return;
		}
		this.m_RefLineDict = oData as Dictionary<int, RefLineData>;

		Draw ();
	}
}
