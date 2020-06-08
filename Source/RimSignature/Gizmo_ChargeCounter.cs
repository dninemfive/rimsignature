using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimSignature
{
    class Gizmo_ChargeCounter : Gizmo
    {
        public CompGadget comp;
        // identical to the shield belt full/empty bar textures
        private static readonly Texture2D ChargeTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D MissingChargeTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.15f, 0.18f));
        private static readonly Texture2D BgTex = SolidColorMaterials.NewSolidColorTexture(Widgets.WindowBGFillColor);
        private const int SpacingWidth = 4;
        private const float minWidth = 100f, maxWidth = 200f, defaultWidth = 140f;

        public Gizmo_ChargeCounter()
        {
            base.order = -100f;
        }
        public override float GetWidth(float maxWidth)
        {
            int charges = comp.MaxCharges;
            return Mathf.Clamp(defaultWidth + (CompGadget.Modifier*(comp.MaxCharges-4)),
                               minWidth, maxWidth);
        }
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect orig = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Gizmo.Height);
            // draw border            
            Widgets.DrawWindowBackground(orig);
            Rect inside = orig.ContractedBy(6f); // couldn't find an appropriate vanilla const, this margin is taken from the shield belt gizmo
            Rect half = inside;
            half.height = orig.height / 2;
            Text.Font = GameFont.Tiny;
            Widgets.Label(half, comp.parent.LabelCap);
            Rect boxContainer = inside;
            boxContainer.yMin = inside.y + inside.height / 2f;
            DrawBoxes(boxContainer, comp.Charges, comp.MaxCharges);
            return new GizmoResult(GizmoState.Clear);
        }
        private void DrawBoxes(Rect inRect, int numFilled, int numBoxes)
        {
            if (numBoxes < 1)
            {
                Log.Error("[RimSignature] Gizmo_ChargeCounter.DrawBoxes called with an unsupported number, " + numBoxes);
                return;
            }
            // else if (numBoxes == 1) return; // 1 is supported but no boxes drawn
            // draw box of background color to hide excess label information
            GUI.DrawTexture(inRect, BgTex);
            float w_b = (inRect.width - (SpacingWidth * (numBoxes - 1))) / numBoxes;
            int width = (int)w_b, 
                offset = (int)(((w_b - width) * numBoxes)/2);
            float xPos = inRect.position.x + offset;
            for(int i = 1; i <= numBoxes; i++)
            {
                Rect cur = new Rect
                {
                    position = new Vector2(xPos, inRect.position.y),
                    height = inRect.height,
                    width = width
                };
                GUI.DrawTexture(cur, i <= numFilled ? ChargeTex : MissingChargeTex);
                xPos += width + SpacingWidth;
            }
        }
    }
}
