using System;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncTest : MonoBehaviour
{
    string base_url = "http://localhost/json.php?";

    [ContextMenu("Request")]
    async void Request()
    {
        for (int i = 0; i < 5; i++)
        {
            string url = $"{base_url}age={i+1}";
            Debug.Log("Waiting 1 second...");
            await Task.Delay(TimeSpan.FromSeconds(1));
            string result = await HttpManager.PostAsync(url);
            Debug.Log($"{i} => {result}");
        }
    }
}
