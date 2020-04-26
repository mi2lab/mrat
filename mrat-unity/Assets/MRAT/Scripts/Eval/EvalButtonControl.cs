using System;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace MRAT
{
	public class EvalButtonControl : MonoBehaviour, IInputClickHandler
	{
		public int ChangeObjectCount = 100;

		private EvalObjectManager _evalManager;
		
		private void Awake()
		{
			_evalManager = FindObjectOfType<EvalObjectManager>();
		}

		void IInputClickHandler.OnInputClicked(InputClickedEventData eventData)
		{
			_evalManager.MaxObjects += ChangeObjectCount;
			_evalManager.ResetObjects();
		}
	}
}
