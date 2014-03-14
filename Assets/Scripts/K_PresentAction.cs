using Extensions;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class K_PresentAction : MonoBehaviour
{
    private LinkedList<K_Present> queue = new LinkedList<K_Present>();
    private LinkedListNode<K_Present> present;
   
    public bool IsWorking { private set; get; }

    public bool RepeatAll { set; get; }

    public static K_PresentAction Ready(MonoBehaviour target){
        return Ready(target.gameObject);}

    public static K_PresentAction Ready(GameObject target) {
        return target.GetComponents<K_PresentAction>().FirstOrDefault(x => !x.IsWorking) ?? target.AddComponent<K_PresentAction>();
    }

    public static K_PresentAction Add(GameObject target) {
        return target.AddComponent<K_PresentAction>();
    }

    K_Present addQueue(K_Present p) {
        queue.AddLast(p);
        if (!IsWorking) StartCoroutine("Working");
        return p;
    }

    public K_Present Next(K_Present p) {
        return addQueue(p); }
    
    public K_Present Next<T>(T to) {
        return addQueue(queue.Last.Value.Next(to)); }

    public K_Present QuickPosition(Vector2 to) {
        return QuickPosition(new Vector3(to.x, to.y, transform.position.z)); }

    public K_Present QuickPosition(Vector3 to) {
        return addQueue(K_Present.Position(transform.position, to)); }

    public K_Present QuickScale(Vector2 to) {
        return QuickScale(new Vector3(to.x, to.y, transform.localScale.z)); }

    public K_Present QuickScale(Vector3 to) {
        return addQueue(K_Present.Scale(transform.localScale, to)); }

    public IEnumerator Working() {
        IsWorking = true;

        present = present != null ? present.Next : queue.First;

        do {
            var p = present.Value;

            while (!p.Start(gameObject))
                yield return null;

            yield return new WaitForSeconds(p.Delay);

            float delta = 0f;
            while (!p.End(gameObject)) {
                delta += 1 / p.Duration * Time.deltaTime;
                p.Work(gameObject, p.Curve.Evaluate(Mathf.Clamp01(delta)));
                yield return null;
            }
        } while ((present = present.Next) != null);
         
        IsWorking = false;
        StopCoroutine("Working");
    }

}