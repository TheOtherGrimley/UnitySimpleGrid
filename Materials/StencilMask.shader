Shader "Custom/Stencil Mask"
{
	Properties{}

	SubShader{

		Tags {
			"Queue" = "Transparent"
		}

		Pass {
			ZWrite Off
		}
	}
}