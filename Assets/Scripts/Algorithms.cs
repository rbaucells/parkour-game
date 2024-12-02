using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Algorithms {

    public static IEnumerator CurveLerp(object instance, string nameOfVariable, float start, float end, AnimationCurve curve, float timeToComplete)
    {
        float elapsedTime = 0f;
        float current = 0.0f;
        Debug.Log("Entered Coroutine");

        var field = instance.GetType().GetField(nameOfVariable);

        while (elapsedTime < timeToComplete)
        {
            // Evaluate the curve at the normalized time (0 to 1)
            float t = elapsedTime / timeToComplete;
            current = Mathf.Lerp(start, end, curve.Evaluate(t));
            
    
            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            field.SetValue(instance, current);
            Debug.Log("Current In Coroutine Is: " + current + " At: " + Time.time);
            // Wait for the next frame
            yield return null;
        }

        // Ensure the value reaches exactly 1 after the loop finishes
        current = Mathf.Lerp(start, end, curve.Evaluate(1));
        field.SetValue(instance, current);
    }
    
    public static IEnumerator CurveLerpVector3(object instance, string nameOfVariable, AnimationCurve curve, float timeToComplete, Vector3 start, Vector3 end)
    {
        // Get the field or property dynamically
        var field = instance.GetType().GetField(nameOfVariable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var property = instance.GetType().GetProperty(nameOfVariable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null && property == null)
        {
            Debug.LogError($"Variable '{nameOfVariable}' not found on object of type '{instance.GetType().Name}'");
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < timeToComplete)
        {
            // Calculate normalized time (0 to 1)
            float t = elapsedTime / timeToComplete;

            // Evaluate the curve and interpolate each component
            float curveValue = curve.Evaluate(t);
            Vector3 currentValue = Vector3.LerpUnclamped(start, end, curveValue);

            // Set the value using reflection
            if (field != null)
                field.SetValue(instance, currentValue);
            else if (property != null)
                property.SetValue(instance, currentValue);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the value reaches exactly the end value
        Vector3 finalValue = Vector3.LerpUnclamped(start, end, curve.Evaluate(1));

        if (field != null)
            field.SetValue(instance, finalValue);
        else if (property != null)
            property.SetValue(instance, finalValue);
    }

    public static void Explode(Vector3 position, float radius, float force, float explosionUpForce)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddExplosionForce(force, position, radius, explosionUpForce);
            }
        }
    }

    public static void Implode(Vector3 position, float radius, float force)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddForce((position - collider.transform.position).normalized * force, ForceMode.Impulse);
            }   
        }
    }
}