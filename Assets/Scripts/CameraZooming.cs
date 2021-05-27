using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZooming : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [Range(0.1f, 5f)]
    [SerializeField] private float zoomSpeed;
    private CinemachineGroupComposer groupComposer;
    private float currentFOV;
    private bool zoomIn = false;
    private bool zoomOut = false;
    private float mouseScroll = 0f;
    public void ZoomInPointerDown()
    {
        zoomIn = true;
    }
    public void ZoomOutPointerDown()
    {
        zoomOut = true;
    }
    public void ZoomInPointerUp()
    {
        zoomIn = false;
    }
    public void ZoomOutPointerUp()
    {
        zoomOut = false;
    }
    // Start is called before the first frame update
    void Awake()
    {
        groupComposer = cam.GetCinemachineComponent<CinemachineGroupComposer>();
        currentFOV = groupComposer.m_MinimumFOV;
    }

    // Update is called once per frame
    void Update()
    {
        if (zoomIn && !zoomOut)
        {
            groupComposer.m_MinimumFOV = Mathf.Clamp(Mathf.Lerp(currentFOV, currentFOV - 5, zoomSpeed * Time.deltaTime),14, 60);
            currentFOV = groupComposer.m_MinimumFOV;
        }
        else if(zoomOut && !zoomIn)
        {
            groupComposer.m_MinimumFOV = Mathf.Clamp(Mathf.Lerp(currentFOV, currentFOV + 5, zoomSpeed * Time.deltaTime), 14, 60);
            currentFOV = groupComposer.m_MinimumFOV;
        }
        else if(!Mathf.Approximately(mouseScroll = Input.GetAxis("Mouse ScrollWheel"),0f))
        {
            groupComposer.m_MinimumFOV = Mathf.Clamp(Mathf.Lerp(currentFOV, currentFOV + (-Mathf.Sign(mouseScroll) * 5), Mathf.Abs(mouseScroll) * 50 * zoomSpeed * Time.deltaTime), 14, 60);
            currentFOV = groupComposer.m_MinimumFOV;
        }
    }
}
