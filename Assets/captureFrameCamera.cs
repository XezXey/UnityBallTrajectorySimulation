using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class captureFrameCamera : MonoBehaviour
{
    // Start is called before the first frame update
    Camera snapCam;
    private IEnumerator coroutine;
    int resWidth = 1920;
    int resHeight = 1080;

    void Start()
    {
        print("from start GGEZ");
        Debug.Log("from start GGEZ");
        while(true)
        {
            CallTakeSnapshot();
        }
    }

    // Update is called once per frame
    void Update()
    {
        print("GGEZ");
        Debug.Log("GGEZ");
        CallTakeSnapshot();
    }

    public void CallTakeSnapshot()
    {
        snapCam = GetComponent<Camera>();
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = snapCam.targetTexture;
        snapCam.Render();
        Texture2D Image = new Texture2D(snapCam.targetTexture.width, snapCam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, snapCam.targetTexture.width, snapCam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        var bytes = Image.EncodeToPNG();
        Destroy(Image);
        snapCam.gameObject.SetActive(false);
        string filename = SnapshotName();
        System.IO.File.WriteAllBytes(filename, bytes);
    }
    string SnapshotName()
    {
        return string.Format("{0}/Snapshots/snap_{1}x{2}_{3}.png", 
            Application.dataPath, 
            resWidth, 
            resHeight, 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
}
