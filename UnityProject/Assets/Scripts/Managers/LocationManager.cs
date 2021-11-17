using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //StartLocation();
    }

    public void StopLocation()
    {
        Input.location.Stop();
    }

    public bool LocationOK = false;
    public IEnumerator GetLocationInfo()
    {
        Utils.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);

        LocationOK = false;

        //先判断是否打开了定位
        //if (!Input.location.isEnabledByUser)
        //{
        //    Debug.LogError("没有打开定位");
        //    yield break;
        //}
        bool isopen = TimSdkManager.Instance.CheckGPSIsOpen();
        Debug.Log($"gps={isopen}");
        if (!isopen)
        {
            Debug.LogError("没有打开定位");
            yield break;
        }

        //这里开起定位计算
        Input.location.Start();
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1)
        {
            Debug.LogError("定位超时");
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("无法使用定位");
            yield break;
        }

        LocationOK = true;
        Debug.Log("经纬度: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " +
                  Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " +
                  Input.location.lastData.timestamp);
        Input.location.Stop();
    }
}
