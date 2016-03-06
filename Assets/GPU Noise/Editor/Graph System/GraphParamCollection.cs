﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace GPUGraph
{
	/// <summary>
	/// A collection of all parameters in a Graph.
	/// </summary>
	[Serializable]
	public struct GraphParamCollection
	{
		public List<ParamNode_Float> FloatParams;
		public List<ParamNode_Texture2D> Tex2DParams;


		/// <summary>
		/// Gets all the parameters in the given Graph.
		/// </summary>
		public GraphParamCollection(Graph g)
		{
			FloatParams = new List<ParamNode_Float>();
			Tex2DParams = new List<ParamNode_Texture2D>();

			foreach (Node n in g.Nodes)
			{
				if (n is ParamNode_Float)
					FloatParams.Add((ParamNode_Float)n);
				else if (n is ParamNode_Texture2D)
					Tex2DParams.Add((ParamNode_Texture2D)n);
			}
		}
		/// <summary>
		/// Gets all parameters from the given collection and recreates them for the given graph.
		/// </summary>
		public GraphParamCollection(Graph otherG, GraphParamCollection c)
			: this(otherG)
		{
			foreach (ParamNode_Float fn in FloatParams)
			{
				int fn2Index = c.FloatParams.FindIndex(fn2 => fn2.Name == fn.Name);
				if (fn2Index == -1)
				{
					Debug.LogError("Couldn't find an original value for scalar var '" + fn.Name + "'");
				}
				else
				{
					fn.DefaultValue = c.FloatParams[fn2Index].DefaultValue;
				}
			}
			foreach (ParamNode_Texture2D tn in Tex2DParams)
			{
				int tn2Index = c.Tex2DParams.FindIndex(tn2 => tn2.Name == tn.Name);
				if (tn2Index == -1)
				{
					Debug.LogError("Couldn't find an original value for Tex2D var '" + tn.Name + "'");
				}
				else
				{
					tn.DefaultVal = c.Tex2DParams[tn2Index].DefaultVal;
				}
			}
		}


		/// <summary>
		/// For any parameters in this collection that exist in the given graph,
		/// the graph parameter's default value is overwritten with this collection's parameter's default value.
		/// </summary>
		public void OverwriteParamValues(Graph g)
		{
			GraphParamCollection gParams = new GraphParamCollection(g);
			
			foreach (ParamNode_Float fn in FloatParams)
			{
				int i = gParams.FloatParams.FindIndex(gFP => gFP.Name == fn.Name);
				if (i >= 0)
				{
					gParams.FloatParams[i].DefaultValue = fn.DefaultValue;
				}
			}
			foreach (ParamNode_Texture2D tn in Tex2DParams)
			{
				int i = gParams.Tex2DParams.FindIndex(gTP => gTP.Name == tn.Name);
				if (i >= 0)
				{
					gParams.Tex2DParams[i].DefaultVal = tn.DefaultVal;
				}
			}
		}
		/// <summary>
		/// Sets the given material to use these parameters, with their default values.
		/// </summary>
		public void SetParams(Material m)
		{
			foreach (ParamNode_Float dat in FloatParams)
			{
				if (!m.HasProperty(dat.Name))
				{
					Debug.LogWarning("Couldn't find property '" + dat.Name +
										"'; Unity may have optimized it out");
				}
				else
				{
					m.SetFloat(dat.Name,
							   (dat.IsSlider ?
									Mathf.Lerp(dat.SliderMin, dat.SliderMax, dat.DefaultValue) :
									dat.DefaultValue));
				}
			}
			foreach (ParamNode_Texture2D dat in Tex2DParams)
			{
				if (!m.HasProperty(dat.Name))
				{
					Debug.LogWarning("Couldn't find property '" + dat.Name +
										"'; Unity may have optimized it out");
				}
				else
				{
					m.SetTexture(dat.Name, dat.DefaultVal);
				}
			}
		}
		/// <summary>
		/// Runs a GUI using EditorGUILayout for these parameters.
		/// This GUI can be used to modify each parameter's "default value" fields.
		/// Returns whether any values have been changed.
		/// </summary>
		public bool ParamEditorGUI()
		{
			bool changed = false;

			foreach (ParamNode_Float fn in FloatParams)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(StringUtils.PrettifyVarName(fn.Name));
				float oldVal = fn.DefaultValue;
				if (fn.IsSlider)
				{
					GUILayout.Label(fn.SliderMin.ToString());
					fn.DefaultValue = Mathf.InverseLerp(fn.SliderMin, fn.SliderMax,
													    GUILayout.HorizontalSlider(Mathf.Lerp(fn.SliderMin,
																							  fn.SliderMax,
																							  fn.DefaultValue),
																				   fn.SliderMin,
																				   fn.SliderMax,
																				   GUILayout.MinWidth(50.0f)));
					GUILayout.Label(fn.SliderMax.ToString());
				}
				else
				{
					fn.DefaultValue = EditorGUILayout.FloatField(fn.DefaultValue);
				}
				
				changed = (changed || Node.AreFloatsDifferent(oldVal, fn.DefaultValue));
				
				GUILayout.EndHorizontal();
			}
			foreach (ParamNode_Texture2D tn in Tex2DParams)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(StringUtils.PrettifyVarName(tn.Name));

				Texture2D oldVal = tn.DefaultVal;
				tn.DefaultVal = (Texture2D)EditorGUILayout.ObjectField(tn.DefaultVal, typeof(Texture2D));

				changed = (oldVal != tn.DefaultVal);

				GUILayout.EndHorizontal();
			}

			return changed;
		}
	}
}