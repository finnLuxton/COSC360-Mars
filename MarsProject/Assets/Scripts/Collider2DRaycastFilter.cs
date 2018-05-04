 using UnityEngine;
 using UnityEngine.UI;

 // Use this script for making non-rectangular buttons. Assign this as a component to a UI button along with
 // a custom 2D collider polygon.

 [RequireComponent (typeof (RectTransform), typeof (Collider2D))]
 public class Collider2DRaycastFilter : MonoBehaviour, ICanvasRaycastFilter 
 {
     Collider2D myCollider;
     RectTransform rectTransform;
     
     void Awake () 
     {
         myCollider = GetComponent<Collider2D>();
         rectTransform = GetComponent<RectTransform>();
     }
  
 #region ICanvasRaycastFilter implementation
     public bool IsRaycastLocationValid (Vector2 screenPos, Camera eventCamera)
     {
         var worldPoint = Vector3.zero;
         var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(
             rectTransform,
             screenPos,
             eventCamera,
             out worldPoint
         );
         if (isInside)
             isInside = myCollider.OverlapPoint(worldPoint);
         return isInside;
     }
 #endregion
 }