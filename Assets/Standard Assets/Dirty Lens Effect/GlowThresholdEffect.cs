using UnityEngine;

// Glow uses the alpha channel as a source of "extra brightness".
// All builtin Unity shaders output baseTexture.alpha * color.alpha, plus
// specularHighlight * specColor.alpha into that.
// Usually you'd want either to make base textures to have zero alpha; or
// set the color to have zero alpha (by default alpha is 0.5).
 
[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu("Image Effects/Dirty Lens")]
public class GlowThresholdEffect : MonoBehaviour
{
	public Quality glowMaskResolution;
	public float blendIntensity;
	public float glowThreshold = 0.25f;
	public float glowIntensity = 1.5f;
	public int blurIterations = 3;
	public float blurSpread = 0.7f;
	public Texture2D overlayTexture;
	public Color glowTint = new Color(1,1,1,0);
	public Texture2D vignette;
	//public Texture2D chrom;
	//public Color blendColor = new Color(1,1,1,0);
	
	public enum Quality{
		Fast = 0,
		Simple = 1,
		Normal = 2,
		Good = 3,
		Fantastic = 4,
	}
	
	RenderTexture buffer;
	RenderTexture buffer2;
	
	private static string compositeMatString =
		@"Shader ""GlowCompose"" {
			Properties {
				_Color (""Glow Amount"", Color) = (1,1,1,1)
				_MainTex ("""", RECT) = ""white"" {}
			}
			SubShader {
				Pass {
					ZTest Always Cull Off ZWrite Off Fog { Mode Off }
					Blend One One
					SetTexture [_MainTex] {constantColor [_Color] combine constant * texture DOUBLE}
				}
			}
			Fallback off
		}";
	
	static Material m_CompositeMaterial = null;
	protected static Material compositeMaterial {
		get {
			if (m_CompositeMaterial == null) {
				m_CompositeMaterial = new Material (compositeMatString);
				m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
				m_CompositeMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_CompositeMaterial;
		} 
	}
	
	
	// --------------------------------------------------------
	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.
	// The alpha value in _Color would normally be 0.25 (to average 4 samples),
	// however if we have glow amount larger than 1 then we increase this.
	
	private static string blurMatString =
		@"Shader ""GlowConeTap"" {
			Properties {
				_Color (""Blur Boost"", Color) = (0,0,0,0.25)
				_MainTex ("""", RECT) = ""white"" {}
			}
			SubShader {
				Pass {
					ZTest Always Cull Off ZWrite Off Fog { Mode Off }
					SetTexture [_MainTex] {constantColor [_Color] combine texture * constant alpha}
					SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}
					SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}
					SetTexture [_MainTex] {constantColor [_Color] combine texture * constant + previous}
				}
			}
			Fallback off
		}";

	static Material m_BlurMaterial = null;
	protected static Material blurMaterial {
		get {
			if (m_BlurMaterial == null) {
				m_BlurMaterial = new Material( blurMatString );
				m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
				m_BlurMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		} 
	}
	
	
	// --------------------------------------------------------
	// The image downsample shaders for each brightness mode.
	// It is in external assets as it's quite complex and uses Cg.
	
	public Shader downsampleShader;
	Material m_DownsampleMaterial = null;
	protected Material downsampleMaterial {
		get {
			if (m_DownsampleMaterial == null) {
				m_DownsampleMaterial = new Material( downsampleShader );
				m_DownsampleMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_DownsampleMaterial;
		} 
	}
	
	public Shader blendShader;
	Material m_BlendMaterial = null;
	protected Material blendMaterial {
		get {
			if (m_BlendMaterial == null) {
				m_BlendMaterial = new Material( blendShader);
				m_BlendMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlendMaterial;
		} 
	}
	
	// --------------------------------------------------------
	//  finally, the actual code
	
	protected void OnDisable()
	{
		if( m_CompositeMaterial ) {
			DestroyImmediate( m_CompositeMaterial.shader );
			DestroyImmediate( m_CompositeMaterial );
		}
		if( m_BlurMaterial ) {
			DestroyImmediate( m_BlurMaterial.shader );
			DestroyImmediate( m_BlurMaterial );
		}
		if( m_DownsampleMaterial )
			DestroyImmediate( m_DownsampleMaterial );
	}
	
	protected void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		
		// Disable the effect if no downsample shader is setup
		if( downsampleShader == null )
		{
			Debug.Log ("No downsample shader assigned! Disabling glow.");
			enabled = false;
		}
		// Disable if any of the shaders can't run on the users graphics card
		else
		{		
			if( !blurMaterial.shader.isSupported )
				enabled = false;
			if( !compositeMaterial.shader.isSupported )
				enabled = false;
			if( !downsampleMaterial.shader.isSupported )
				enabled = false;
		}
	}
	
	// Performs one blur iteration.
	public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
	{
		RenderTexture.active = dest;
		blurMaterial.SetTexture("_MainTex", source);
		
		float offsetX = (.5F+iteration*blurSpread) / (float)source.width;
		float offsetY = (.5F+iteration*blurSpread) / (float)source.height;
		GL.PushMatrix ();
		GL.LoadOrtho ();    
		
		for (int i = 0; i < blurMaterial.passCount; i++) {
			blurMaterial.SetPass (i);
			Render4TapQuad( dest, offsetX, offsetY );
		}
		GL.PopMatrix ();

	}
	
	// Downsamples the texture to a quarter resolution.
	private void DownSample4x (RenderTexture source, RenderTexture dest)
	{
		// Remove colors below the threshold.
		downsampleMaterial.SetFloat("_GlowThreshold", glowThreshold);
		downsampleMaterial.color = new Color( glowTint.r, glowTint.g, glowTint.b, glowTint.a/4.0f );
		//ImageEffects.BlitWithMaterial( , source, dest );
		//blendMaterial.SetColor("_Color", blendColor);
		Graphics.Blit (source, dest, downsampleMaterial);
	}
	
	// Called by the camera to apply the image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		// Clamp parameters to sane values
		glowThreshold = Mathf.Clamp (glowThreshold, 0.0f, 1.0f );
		glowIntensity = Mathf.Clamp( glowIntensity, 0.0f, 10.0f );
		blurIterations = Mathf.Clamp( blurIterations, 0, 30 );
		blurSpread = Mathf.Clamp( blurSpread, 0.5f, 1.0f );
		
		switch(glowMaskResolution){
			case Quality.Fast:
				 buffer = RenderTexture.GetTemporary(source.width/8, source.height/8, 0);
				 buffer2 = RenderTexture.GetTemporary(source.width/8, source.height/8, 0);
			break;
			case Quality.Simple:
				 buffer = RenderTexture.GetTemporary(source.width/6, source.height/6, 0);
				 buffer2 = RenderTexture.GetTemporary(source.width/6, source.height/6, 0);
			break;
			case Quality.Normal:
				 buffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
				 buffer2 = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
			break;
			case Quality.Good:
				 buffer = RenderTexture.GetTemporary(source.width/2, source.height/2, 0);
				 buffer2 = RenderTexture.GetTemporary(source.width/2, source.height/2, 0);
			break;
			case Quality.Fantastic:
				 buffer = RenderTexture.GetTemporary(source.width, source.height, 0);
				 buffer2 = RenderTexture.GetTemporary(source.width, source.height, 0);
			break;
		}
		// Copy source to the 4x4 smaller texture.
		DownSample4x (source, buffer);
		
		// Blur the small texture
		float extraBlurBoost = Mathf.Clamp01( (glowIntensity - 1.0f) / 4.0f );
		blurMaterial.color = new Color( 1F, 1F, 1F, 0.25f + extraBlurBoost );
		
		bool oddEven = true;
		for(int i = 0; i < blurIterations; i++)
		{
			if( oddEven )
				FourTapCone (buffer, buffer2, i);
			else
				FourTapCone (buffer2, buffer, i);
			oddEven = !oddEven;
		}
		
		if( oddEven ){
			BlitGlow(buffer, destination);
			blendMaterial.SetFloat ("_Intensity1", blendIntensity);
			blendMaterial.SetTexture ("_Mask", buffer);
			blendMaterial.SetTexture ("_Overlay1", overlayTexture);
			blendMaterial.SetTexture ("_Vignette", vignette);
			//blendMaterial.SetTexture ("_Chrom", chrom);
			
			Graphics.Blit (source, destination, blendMaterial);
		}
		else{
			BlitGlow(buffer2, destination);
			blendMaterial.SetFloat ("_Intensity1", blendIntensity);
			blendMaterial.SetTexture ("_Mask", buffer2);
			blendMaterial.SetTexture ("_Overlay1", overlayTexture);
			blendMaterial.SetTexture ("_Vignette", vignette);
			//blendMaterial.SetTexture ("_Chrom", chrom);
			
			Graphics.Blit (source, destination, blendMaterial);	
		}
		

		
		RenderTexture.ReleaseTemporary(buffer);
		RenderTexture.ReleaseTemporary(buffer2);
		
	}
	
	public void BlitGlow( RenderTexture source, RenderTexture dest )
	{
		compositeMaterial.color = new Color(1F, 1F, 1F, Mathf.Clamp01(glowIntensity));
		Graphics.Blit(source, dest, compositeMaterial);
	}


	private static void Render4TapQuad( RenderTexture dest, float offsetX, float offsetY )
	{
		GL.Begin( GL.QUADS ); 
		
		// Direct3D needs interesting texel offsets!		
		Vector2 off = Vector2.zero;
		if( dest != null )
			off = dest.GetTexelOffset() * 0.75f;
		
		Set4TexCoords( off.x, off.y, offsetX, offsetY );
		GL.Vertex3( 0,0, .1f );
		
		Set4TexCoords( 1.0f + off.x, off.y, offsetX, offsetY );
		GL.Vertex3( 1,0, .1f );
		
		Set4TexCoords( 1.0f + off.x, 1.0f + off.y, offsetX, offsetY );
		GL.Vertex3( 1,1,.1f );
		
		Set4TexCoords( off.x, 1.0f + off.y, offsetX, offsetY );
		GL.Vertex3( 0,1,.1f );
		
		GL.End();
	}
	
	private static void Set4TexCoords( float x, float y, float offsetX, float offsetY )
	{
		GL.MultiTexCoord2( 0, x - offsetX, y - offsetY );
		GL.MultiTexCoord2( 1, x + offsetX, y - offsetY );
		GL.MultiTexCoord2( 2, x + offsetX, y + offsetY ); 
		GL.MultiTexCoord2( 3, x - offsetX, y + offsetY );
	}
}
