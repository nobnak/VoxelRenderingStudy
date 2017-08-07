﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gist;
using Gist.Extensions.AABB;

[ExecuteInEditMode]
public class SliceVisualizer : MonoBehaviour {

	[SerializeField]
	Material sliceByPointMat;
    [SerializeField]
    Color boundColor = Color.green;
    [SerializeField]
    Color voxelColor = Color.green;
    [SerializeField]
    ComputeShaderLinker csLinker;

	Texture voxelTex;
    VoxelTexture accumulatedTex;
	AbstractVoxelBounds voxelBounds;
	ShaderConstants shaderConstants;
    ViewSpaceBounds viewSpaceBounds;

	#region Unity
	void OnEnable() {
		shaderConstants = ShaderConstants.Instance;
        viewSpaceBounds = new ViewSpaceBounds ();
	}
    void Update() {
        var view = Camera.main.transform.worldToLocalMatrix;
        viewSpaceBounds.Init (voxelBounds, transform.localToWorldMatrix);
    }
	void OnRenderObject() {
		if (!IsInitialized)
			return;

        var depth = 2 * voxelTex.width;

        var view = Camera.current.transform.worldToLocalMatrix;
        var model = view.inverse;
        var boundsUvToVoxelUv = viewSpaceBounds.ViewUvToVoxelUv;
        var voxelUvToLocal = viewSpaceBounds.ViewUvToLocal;
        viewSpaceBounds.SetView (view);

        sliceByPointMat.SetFloat (shaderConstants.PROP_VERTEX_TO_DEPTH, 1f / depth);
        sliceByPointMat.SetMatrix (shaderConstants.PROP_BOUNDS_UV_TO_VOXEL_UV, boundsUvToVoxelUv);
        sliceByPointMat.SetMatrix (shaderConstants.PROP_BOUNDS_UV_TO_LOCAL, voxelUvToLocal);
        sliceByPointMat.SetMatrix (shaderConstants.PROP_BOUNDS_MODEL, model);
		sliceByPointMat.SetTexture (shaderConstants.PROP_VOXEL_COLOR_TEX, voxelTex);
		sliceByPointMat.SetPass (0);
        Graphics.DrawProcedural (MeshTopology.Points, depth);
	}
    void OnDrawGizmos() {
        if (!IsInitialized)
            return;
        
        viewSpaceBounds.SetView (Camera.current.transform.worldToLocalMatrix);
        viewSpaceBounds.DrawGizmos (voxelBounds, voxelColor, boundColor);
    }
	#endregion

    public void UpdateVoxelTexture(RenderTexture tex) {
		this.voxelTex = tex;
        PrepareAccumTex (tex.width);

	}
	public void Set(AbstractVoxelBounds voxelBounds) {
		this.voxelBounds = voxelBounds;
	}
	public bool IsInitialized {
        get { 
            return voxelTex != null
            && voxelBounds != null
            && accumulatedTex != null
            && isActiveAndEnabled; 
        }
	}

    void PrepareAccumTex (int resolution) {
        if (accumulatedTex == null) {
            accumulatedTex = new VoxelTexture (resolution, RenderTextureFormat.ARGB32);
            csLinker.Cleaner.Clear (accumulatedTex.Texture);
        }
        accumulatedTex.SetResolution (resolution);
    }
}
