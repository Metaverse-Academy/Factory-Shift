// using UnityEngine;

// public class EyeFollowerRobust : MonoBehaviour
// {
//     public Transform eye;                       // Transform الخاص بالعين (أو البؤبؤ)
//     public Camera cam;                          // لو تبي كاميرا غير MainCamera
//     public float rotationSpeed = 8f;            // سلاسة الحركة
//     public float rayDistance = 10f;             // نقطة الهدف على الري
//     public bool initialFaceCamera = true;       // عند البداية: اجعل العين تواجه الكاميرا تلقائياً

//     // محور العين المحلي الذي يجب أن يشير إلى الهدف (جرب القيم حتى تناسب الموديل)
//     public enum LocalAxis { X, NegX, Y, NegY, Z, NegZ }
//     public LocalAxis localForward = LocalAxis.Z;

//     // انعكاسات عند الحاجة
//     public bool invertX = false;
//     public bool invertY = false;

//     // معايرة إضافية (أي دوران ثابت تريده بعد الحساب)
//     public Vector3 extraEulerOffset = Vector3.zero;

//     // داخلياً
//     Quaternion initialRotation;
//     Vector3 initialWorldAxis; // اتجاه الـ localAxis في العالم بعد حفظ initialRotation

//     void Start()
//     {
//         if (eye == null) eye = transform;
//         if (cam == null) cam = Camera.main;

//         // حفظ الدوران الابتدائي
//         initialRotation = eye.rotation;

//         // اتجاه المحور المحلي في مساحة العالم بحسب الدوران الابتدائي
//         Vector3 localAxis = GetLocalAxisVector(localForward);
//         initialWorldAxis = initialRotation * localAxis;

//         // خيار: قلب العين لمواجهة الكاميرا عند البداية
//         if (initialFaceCamera && cam != null)
//         {
//             Vector3 dirToCam = (cam.transform.position - eye.position).normalized;
//             if (dirToCam.sqrMagnitude > 0.0001f)
//             {
//                 Quaternion delta = Quaternion.FromToRotation(initialWorldAxis, dirToCam);
//                 initialRotation = delta * initialRotation;
//                 initialWorldAxis = initialRotation * localAxis;
//                 eye.rotation = initialRotation;
//             }
//         }
//     }

//     void Update()
//     {
//         if (cam == null) return;

//         // نقطة الهدف على الري من الماوس
//         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//         Vector3 targetPoint = ray.GetPoint(rayDistance);
//         Vector3 dir = targetPoint - eye.position;

//         // تطبيق انعكاسات المحاور اختيارياً قبل التطبيع
//         if (invertX) dir.x = -dir.x;
//         if (invertY) dir.y = -dir.y;

//         if (dir.sqrMagnitude < 0.0001f) return;
//         dir = dir.normalized;

//         // حساب دوران من الوضع الابتدائي بحيث يجعل الـ initialWorldAxis يشير إلى dir
//         Quaternion fromTo = Quaternion.FromToRotation(initialWorldAxis, dir);
//         Quaternion targetRotation = fromTo * initialRotation;

//         // تطبيق أزاحةEuler إضافية (مريحة للموديلات الغريبة)
//         if (extraEulerOffset != Vector3.zero)
//         {
//             targetRotation *= Quaternion.Euler(extraEulerOffset);
//         }

//         // سلاسة
//         eye.rotation = Quaternion.Slerp(eye.rotation, targetRotation, Time.deltaTime * rotationSpeed);
//     }

//     // تحويل enum إلى Vector3 محلي
//     Vector3 GetLocalAxisVector(LocalAxis a)
//     {
//         switch (a)
//         {
//             case LocalAxis.X: return Vector3.right;
//             case LocalAxis.NegX: return Vector3.left;
//             case LocalAxis.Y: return Vector3.up;
//             case LocalAxis.NegY: return Vector3.down;
//             case LocalAxis.Z: return Vector3.forward;
//             case LocalAxis.NegZ: return Vector3.back;
//         }
//         return Vector3.forward;
//     }

//     // للتصحيح اليدوي: ارسم خط للاختبار (اختياري)
//     void OnDrawGizmosSelected()
//     {
//         if (eye == null) return;
//         Gizmos.color = Color.cyan;
//         Vector3 axis = (Application.isPlaying ? (initialRotation * GetLocalAxisVector(localForward)) : (eye.rotation * GetLocalAxisVector(localForward)));
//         Gizmos.DrawLine(eye.position, eye.position + axis.normalized * 0.5f);
//     }
// }

using UnityEngine;

public class EyeFollowerRobust : MonoBehaviour
{
    [Header("Eye Target")]
    public Transform eye;                       // Transform الخاص بالعين (أو البؤبؤ)
    public Camera cam;                          // اتركه فارغ = MainCamera
    public float rotationSpeed = 8f;            // سلاسة حركة التتبع
    public float rayDistance = 10f;             // مدى نقطة الهدف
    public bool initialFaceCamera = true;       // اجعل العين تواجه الكاميرا عند البداية

    // المحور المحلي الذي يجب أن يشير إلى الهدف
    public enum LocalAxis { X, NegX, Y, NegY, Z, NegZ }
    public LocalAxis localForward = LocalAxis.Z;

    [Header("Axis Inversion")]
    public bool invertX = false;
    public bool invertY = false;

    [Header("Fixed Euler Offset")]
    public Vector3 extraEulerOffset = Vector3.zero;

    // حركة طفوية (أعلى/أسفل)
    [Header("Floating Motion")]
    public bool enableFloating = true;
    public float floatAmplitude = 0.05f;  // مقدار الصعود والنزول
    public float floatSpeed = 1.5f;       // السرعة

    // داخلياً
    Quaternion initialRotation;
    Vector3 initialWorldAxis;
    Vector3 basePosition;
    float floatTimer;

    void Start()
    {
        if (eye == null) eye = transform;
        if (cam == null) cam = Camera.main;

        // حفظ الدوران الأصلي
        initialRotation = eye.rotation;

        // حفظ موقع البداية (حركة الطفو)
        basePosition = eye.localPosition;

        // اتجاه المحور المحلي في مساحة العالم
        Vector3 localAxis = GetLocalAxisVector(localForward);
        initialWorldAxis = initialRotation * localAxis;

        // اجعل العين تواجه الكاميرا عند البداية
        if (initialFaceCamera && cam != null)
        {
            Vector3 dirToCam = (cam.transform.position - eye.position).normalized;
            if (dirToCam.sqrMagnitude > 0.0001f)
            {
                Quaternion delta = Quaternion.FromToRotation(initialWorldAxis, dirToCam);
                initialRotation = delta * initialRotation;
                initialWorldAxis = initialRotation * localAxis;
                eye.rotation = initialRotation;
            }
        }
    }

    void Update()
    {
        HandleEyeFollow();
        HandleFloating();
    }

    void HandleEyeFollow()
    {
        if (cam == null) return;

        // الهدف من مؤشر الفأرة
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint = ray.GetPoint(rayDistance);
        Vector3 dir = targetPoint - eye.position;

        // انعكاسات
        if (invertX) dir.x = -dir.x;
        if (invertY) dir.y = -dir.y;

        if (dir.sqrMagnitude < 0.0001f) return;
        dir = dir.normalized;

        // تحويل من المحور الابتدائي إلى الاتجاه الجديد
        Quaternion fromTo = Quaternion.FromToRotation(initialWorldAxis, dir);
        Quaternion targetRotation = fromTo * initialRotation;

        // أي تعديل إضافي
        if (extraEulerOffset != Vector3.zero)
            targetRotation *= Quaternion.Euler(extraEulerOffset);

        // سلاسة
        eye.rotation = Quaternion.Slerp(
            eye.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }

    void HandleFloating()
    {
        if (!enableFloating) return;

        floatTimer += Time.deltaTime * floatSpeed;
        float offset = Mathf.Sin(floatTimer) * floatAmplitude;

        Vector3 pos = basePosition;
        pos.y += offset;
        eye.localPosition = pos;
    }

    // تحويل enum → Vector3
    Vector3 GetLocalAxisVector(LocalAxis a)
    {
        switch (a)
        {
            case LocalAxis.X: return Vector3.right;
            case LocalAxis.NegX: return Vector3.left;
            case LocalAxis.Y: return Vector3.up;
            case LocalAxis.NegY: return Vector3.down;
            case LocalAxis.Z: return Vector3.forward;
            case LocalAxis.NegZ: return Vector3.back;
        }
        return Vector3.forward;
    }
}
