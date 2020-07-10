using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carrom
{
    public static class EventSystemsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 WorldPosition(this PointerEventData eventData)
        {
            return eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 DragDisplacement(this PointerEventData eventData, Vector2 startPosition)
        {
            return (eventData.position - startPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DragAngle(this PointerEventData eventData, Vector2 startPosition)
        {
            var direction = eventData.DragDisplacement(startPosition);
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float OppositeDragAngle(this PointerEventData eventData, Vector2 startPosition)
        {
            var direction = -eventData.DragDisplacement(startPosition);
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DragDisplacementWorld(this PointerEventData eventData, Vector3 worldStartPosition)
        {
            return (eventData.WorldPosition() - worldStartPosition);
        }
    }
}