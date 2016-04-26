using System;
using UnityEngine;

namespace UnityEditor
{
	internal class FontDissolveShaderGUI : SpriteDissolveShaderGUI
	{
		public override void DoAlbedoArea()
		{

		}

		public override void ShowDissolveMap(Material material)
		{
			if (material.GetTexture("_DissolveMap") != null) {
				EditorGUI.indentLevel += (MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
				m_Tiling.x = tilingX.floatValue;
				m_Tiling.y = tilingY.floatValue;
				EditorGUI.BeginChangeCheck();
				m_Tiling = EditorGUILayout.Vector2Field(Styles.tiling, m_Tiling);
				if (EditorGUI.EndChangeCheck()) {
					tilingX.floatValue = m_Tiling.x;
					tilingY.floatValue = m_Tiling.y;
					
				}
				EditorGUI.indentLevel -= (MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
			}
			m_MaterialEditor.ShaderProperty(dissolveAmount, Styles.dissolveAmountText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
		}
	}

} // namespace UnityEditor
