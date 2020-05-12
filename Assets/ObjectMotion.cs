using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;

[RequireComponent(typeof(Rigidbody))]
[System.Serializable]
public class cameraParameters
{
    public int width {get; set;}
    public int height {get; set;}
    public float[] projectionMatrix {get; set;}
    public float[] worldToCameraMatrix {get; set;}
}

[System.Serializable]
public class allCameraParameters{
    public string[] col_names = {"ball_world_x", "ball_world_y", "ball_world_z", "ball_velocity", "ball_screen_main_x", "ball_screen_main_y", "ball_screen_main_z", 
                            "ball_ndc_main_x", "ball_ndc_main_y", "ball_ndc_main_z", "ball_screen_along_x", "ball_screen_along_y", "ball_screen_along_z", 
                            "ball_ndc_along_x", "ball_ndc_along_y", "ball_ndc_along_z", "ball_screen_top_x", "ball_screen_top_y", "ball_screen_top_z", 
                            "ball_ndc_top_x", "ball_ndc_top_y", "ball_ndc_top_z", "fx", "fy", "fz", "add_force_flag", "trajectory_type", "outside_flag", "t"};
    public cameraParameters mainCameraParams;
    public cameraParameters alongPitchCameraParams;
    public cameraParameters topPitchCameraParams;
}
public class ObjectMotion : MonoBehaviour
{
    // Declare Camera objects
    Camera mainCamera;
    Camera alongPitchCamera;
    Camera topPitchCamera;
    // Variables
    protected Rigidbody rb; //Ball rigidbody
    private int t=0;    //Time step to record
    private Vector3 force;  //Force to apply 
    private Vector3 direction;  //Direction of force
    public string trajectoryType = "MagnusProjectile"; // 3 different kind of trajectory : 1)Rolling, 2)Projectile, 3)MagnusProjectile
    public bool addForceFlag = false;   //Set when applied force
    private float magnusForceWeight = 0.01f;    //Magnus force weight to reduce once ball bounce on a floor.
    public bool outsideFlag = false;    //Set when the ball is outside (Trajectory is not continuous.)
    public bool bounceFlag = false;  //Set this for make it bounce or not(Can also vary bounceness in Unity Editor)
    public int trial = 1;   //For file prefix save
    public bool debug = false;  // For print a debug message
    // Start is called before the first frame update
    void Start()
    {
        // Set the screen resolution
        Screen.SetResolution(1920, 1080, true);
        // Get the rigidbody component to rb
        rb = GetComponent<Rigidbody>();
        // Random Initailize the position
        //Vector3 initialPosition = new Vector3(Random.Range(-20.0f, 20.0f), 0.727f, Random.Range(-10.0f, 10.0f));
        //rb.transform.position = initialPosition;
        // Transform the eulerAngles of ball for make it curve
        rb.transform.eulerAngles = new Vector3(0.0f, 15.0f, 0.0f);
        // Initialize the dataset folder and save the camera parameters
        InitilizeDatasetFolder();
        WriteCameraParametersToFile();
    }
    void InitilizeDatasetFolder(){
        // Initialize the Dataset Folder if not exists
        string savePath = string.Format("{0}/SimulatedTrajectory/Trial_{1}/", Application.dataPath, trial);
        FileInfo file = new FileInfo(savePath);
        if(!file.Directory.Exists){
            System.IO.Directory.CreateDirectory(file.DirectoryName);
        }
    }

    public float[] Matrix4x4ToArray(Matrix4x4 mat){
        // Convert type of camera parameters (Matrix4x4 to array)
        float[] matArray = new float[16]; 
        for (int i=0; i<4; i++)
        {
            for (int j=0; j<4; j++)
            {
                matArray[i*4 + j] = mat[i, j];
            }
        }
        return matArray;
    }
    void WriteCameraParametersToFile(){
        // Write Camera parameters to .json format
        // Get the used camera component to Camera object by name
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        alongPitchCamera = GameObject.Find("Along Pitch Camera").GetComponent<Camera>(); 
        topPitchCamera = GameObject.Find("Top Pitch Camera").GetComponent<Camera>();

        // Save the camera parameters by serialize from class object to string
        allCameraParameters allCameraParams = new allCameraParameters(){
            mainCameraParams = new cameraParameters(){
                width = mainCamera.pixelWidth,
                height = mainCamera.pixelHeight,
                projectionMatrix = Matrix4x4ToArray(mainCamera.projectionMatrix),
                worldToCameraMatrix = Matrix4x4ToArray(mainCamera.worldToCameraMatrix)
            },
            topPitchCameraParams = new cameraParameters(){
                width = topPitchCamera.pixelWidth,
                height = topPitchCamera.pixelHeight,
                projectionMatrix = Matrix4x4ToArray(topPitchCamera.projectionMatrix),
                worldToCameraMatrix = Matrix4x4ToArray(topPitchCamera.worldToCameraMatrix)
            },
            alongPitchCameraParams = new cameraParameters(){
                width = alongPitchCamera.pixelWidth,
                height = alongPitchCamera.pixelHeight,
                projectionMatrix = Matrix4x4ToArray(alongPitchCamera.projectionMatrix),
                worldToCameraMatrix = Matrix4x4ToArray(alongPitchCamera.worldToCameraMatrix)
            }
        };
        string json = JsonConvert.SerializeObject(allCameraParams);
        File.WriteAllText(getPathCameraParamters("configFile"), json);
    }

    void WriteTrajectoryToFile(Vector3 ball_screen_coordinate_main, Vector3 ball_ndc_coordinate_main,
                                Vector3 ball_screen_coordinate_along, Vector3 ball_ndc_coordinate_along,
                                Vector3 ball_screen_coordinate_top, Vector3 ball_ndc_coordinate_top,
                                Vector3 force, int t)
    {
        using (var sw = new StreamWriter(getPath(), append:true))
        {
        string traj=string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28}",
                rb.position.x, rb.position.y, rb.position.z, rb.velocity.magnitude, ball_screen_coordinate_main.x, ball_screen_coordinate_main.y, 
                ball_screen_coordinate_main.z, ball_ndc_coordinate_main.x, ball_ndc_coordinate_main.y, ball_ndc_coordinate_main.z,
                ball_screen_coordinate_along.x, ball_screen_coordinate_along.y, ball_screen_coordinate_along.z, ball_ndc_coordinate_along.x, 
                ball_ndc_coordinate_along.y, ball_ndc_coordinate_along.z, ball_screen_coordinate_top.x, ball_screen_coordinate_main.y, 
                ball_screen_coordinate_top.z, ball_ndc_coordinate_top.x, ball_ndc_coordinate_top.y, ball_ndc_coordinate_top.z, force.x, force.y, force.z,
                addForceFlag, trajectoryType, outsideFlag, t
            );
            sw.WriteLine(traj);
        }
    }

    private string getPathCameraParamters(string cameraPosition)
    {
        return string.Format("{0}/SimulatedTrajectory/Trial_{1}/{2}_camParams_Trial{3}.json", Application.dataPath, trial, cameraPosition, trial);
    }
    private string getPath()
    {
        return string.Format("{0}/SimulatedTrajectory/Trial_{1}/{2}Trajectory_Trial{3}.csv", Application.dataPath, trial, trajectoryType, trial);
    }

    Vector3 ManualScreenPointToWorldPoint(Vector2 screenPoint, float distance, Camera cam) {
        // here we are converting screen point in screen space to camera space point placed on a plane "distance" away from the camera
        // screen point is in range [(0,0) - (Screen.Width, Screen.Height)]
        // Pipeline : 
        // object space -> {MODEL} -> World Space -> {VIEW} -> Eye Space -> {PROJ} -> Clip Space -> {perspective divide} -> NDC -> {Viewport/DepthRange} -> Window Space
        // Move 3rd row to 2nd row and replace 3rd row with (0, 0, 0, 1)
        Matrix4x4 projectionMatrix = cam.projectionMatrix;
        print(projectionMatrix);
        projectionMatrix.SetRow(2, projectionMatrix.GetRow(3));
        print(projectionMatrix);
        projectionMatrix.SetRow(3, new Vector4(.0f, .0f, .0f, 1.0f));
        print(projectionMatrix);
        Matrix4x4 projectionMatrixInverse = projectionMatrix.inverse;
        print(projectionMatrix);
        Matrix4x4 cameraToWorldMatrix = cam.cameraToWorldMatrix;
        Vector2 pointViewportSpace = screenPoint / new Vector2(Screen.width, Screen.height); // convert space [(0,0) - (Screen.Width, Screen.Height)] to [(0,0) - (1,1)]
        Vector2 pointCameraSpaceNormalized = (pointViewportSpace * 2.0f) - Vector2.one; // convert space [(0,0) - (1,1)] to [(-1,-1) - (1,1)]
        Vector2 pointCameraSpace = pointCameraSpaceNormalized * distance; // convert space [(-1,-1) - (1,1)] to [(-dist,-dist) - (dist, dist)]
        Vector4 planePoint = new Vector4(pointCameraSpace.x, pointCameraSpace.y, distance, 1.0f); // define the point (don't know why z and w components need to be set to distance)
        // calculate convertion matrix from camera space to world space
        Matrix4x4 matrix = cameraToWorldMatrix * projectionMatrixInverse;
        // multiply world point by VP matrix
        Vector4 worldPoint = matrix * planePoint;
        return worldPoint;
    }

    Vector3 manualWorldToScreenPoint(Vector3 wp, Camera cam) {
        // calculate view-projection matrix
        Matrix4x4 mat = cam.projectionMatrix * cam.worldToCameraMatrix;
        // multiply world point by VP matrix
        Vector4 temp = mat * new Vector4(wp.x, wp.y, wp.z, 1f);

        if (temp.w == 0f) {
            // point is exactly on camera focus point, screen point is undefined
            // unity handles this by returning 0,0,0
            return Vector3.zero;
        } else {
            // convert x and y from clip space to window coordinates
            temp.x = (temp.x/temp.w + 1f)*.5f * cam.pixelWidth;
            temp.y = (temp.y/temp.w + 1f)*.5f * cam.pixelHeight;
            temp.z = temp.z + (2 * cam.nearClipPlane);  // Depth from camera plane(exlcude nearClipPlane) to object
            return new Vector3(temp.x, temp.y, temp.w);
        }
     }
    void Update(){
        if (trajectoryType == "MagnusProjectile"){
            bool spinDirection = (Random.value > 0.5f);
            MagnusEffect(force, rb, spinDirection, direction);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {   
        Time.timeScale = 50.0f;
        // Main camera
        Vector3 ball_screen_coordinate_main = mainCamera.WorldToScreenPoint(rb.position);
        Vector3 ball_ndc_coordinate_main = mainCamera.WorldToViewportPoint(rb.position);
        // Along pitch camera
        Vector3 ball_screen_coordinate_along = alongPitchCamera.WorldToScreenPoint(rb.position);
        Vector3 ball_ndc_coordinate_along = alongPitchCamera.WorldToViewportPoint(rb.position);
        // Top pitch camera
        Vector3 ball_screen_coordinate_top = topPitchCamera.WorldToScreenPoint(rb.position);
        Vector3 ball_ndc_coordinate_top = topPitchCamera.WorldToViewportPoint(rb.position);
        if(debug){
            print("Force : " + force);
            print("Direction : " + direction);
            print("Position : " + rb.position);
            //print("Position in screen : " + mainCamera.WorldToScreenPoint(rb.position));
            //print("World to screen function(mainCamera) : " + manualWorldToScreenPoint(rb.position, mainCamera));
            //print("World to screen method(mainCamera) : " + mainCamera.WorldToScreenPoint(rb.position));
            //print("World to screen function(alongPitchCamera) : " + manualWorldToScreenPoint(rb.position, alongPitchCamera));
            //print("World to screen method(alongPitchCamera) : " + alongPitchCamera.WorldToScreenPoint(rb.position));
            //print("World to screen function(topPitchCamera) : " + manualWorldToScreenPoint(rb.position, topPitchCamera));
            //print("World to screen method(topPitchCamera) : " + topPitchCamera.WorldToScreenPoint(rb.position));
            Vector3 rbOnScreenPos_main_fn = manualWorldToScreenPoint(rb.position, mainCamera);
            Vector3 rbOnScreenPos_along_fn = manualWorldToScreenPoint(rb.position, alongPitchCamera);
            Vector3 rbOnScreenPos_top_fn = manualWorldToScreenPoint(rb.position, topPitchCamera);
            //print("Position in world : " + rb.position);
            //print("Screen to world function (mainCamera) : " + ManualScreenPointToWorldPoint(new Vector2(rbOnScreenPos_main_fn.x, rbOnScreenPos_main_fn.y), rbOnScreenPos_main_fn.z, mainCamera));
            //print("Screen to world method (mainCamera) : " + mainCamera.ScreenToWorldPoint(new Vector3(rbOnScreenPos_main_fn.x, rbOnScreenPos_main_fn.y, rbOnScreenPos_main_fn.z)));
            //print("Screen to world function (alongPitchCamera) : " + ManualScreenPointToWorldPoint(new Vector2(rbOnScreenPos_along_fn.x, rbOnScreenPos_along_fn.y), rbOnScreenPos_along_fn.z, alongPitchCamera));
            //print("Screen to world method (alongPitchCamera) : " + alongPitchCamera.ScreenToWorldPoint(new Vector3(rbOnScreenPos_along_fn.x, rbOnScreenPos_along_fn.y, rbOnScreenPos_along_fn.z)));
            //print("Screen to world function (topPitchCamera) : " + ManualScreenPointToWorldPoint(new Vector2(rbOnScreenPos_top_fn.x, rbOnScreenPos_top_fn.y), rbOnScreenPos_top_fn.z, topPitchCamera));
            //print("Screen to world method (topPitchCamera) : " + topPitchCamera.ScreenToWorldPoint(new Vector3(rbOnScreenPos_top_fn.x, rbOnScreenPos_top_fn.y, rbOnScreenPos_top_fn.z)));
            //print("==============================================================================================");
        }
        //print(rb.position.y + ", Speed : " + rb.velocity.sqrMagnitude);
        if (rb.velocity.sqrMagnitude <= 0.3f && rb.position.y <= 0.3f && rb.position.y >= -0.5f)   // Check wheter ball is on the floor and almost stop
        {
            if (trajectoryType == "Projectile" || trajectoryType == "MagnusProjectile"){
                force = Projectile(force);
            }
            else if (trajectoryType == "Rolling"){
                force = Rolling(force);
            }
            else if (trajectoryType == "Mixed"){
                float mixed_selection = Random.Range(0.0f, 1.0f);
                if (mixed_selection >= 0.5){
                    force = Projectile(force);
                }
                else{
                    force = Rolling(force);
                }
            }
            print("Force after random: " + force);
            magnusForceWeight = 0.25f;
            direction = FindDirection(rb.position);
            force = Vector3.Scale(force, direction);
            rb.AddForce(force, ForceMode.Impulse);
            addForceFlag = true;
        }
        // Write the data every timestep
        WriteTrajectoryToFile(ball_screen_coordinate_main, ball_ndc_coordinate_main, ball_screen_coordinate_along, ball_ndc_coordinate_along, 
                            ball_screen_coordinate_top, ball_ndc_coordinate_top, force, t);
        addForceFlag = false;
        t++;
        if (rb.position.y < -1.0f){
            // Replay when ball is outside the field
            outsideFlag = true;
            WriteTrajectoryToFile(ball_screen_coordinate_main, ball_ndc_coordinate_main, ball_screen_coordinate_along, ball_ndc_coordinate_along, 
                            ball_screen_coordinate_top, ball_ndc_coordinate_top, force, t);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //#if UNITY_EDITOR
            //    UnityEditor.EditorApplication.isPlaying = false;
            //#endif
        }
    }

    void OnCollisionEnter(Collision collision){
        // Once a collision happens reduce the magnusForce and can set bounceFlag for control bounce of the ball
        magnusForceWeight = 0.01f;
        if (!bounceFlag){
            rb.velocity = new Vector3(.0f, .0f, .0f);
        }
    }

    Vector3 FindDirection(Vector3 ball_position){
        // Find direction that won't make ball goes out of the field.
        print("Find direction (ball_position) : " + ball_position);
        Vector3 normalize_rb_position = new Vector3((ball_position.x + Mathf.Epsilon)/Mathf.Abs(ball_position.x + Mathf.Epsilon), (ball_position.y + Mathf.Epsilon)/Mathf.Abs(ball_position.y + Mathf.Epsilon), (ball_position.z + Mathf.Epsilon)/Mathf.Abs(ball_position.z + Mathf.Epsilon));
        Vector3 direction = new Vector3(-normalize_rb_position.x, 1.0f, -normalize_rb_position.z);
        return direction;
    }

    Vector3 Projectile(Vector3 force){
        // Projectile a ball force
        force.x = 0.0f; //Random.Range(3.0f, 10.0f);
        force.y = Random.Range(5.0f, 15.0f);
        force.z = Random.Range(3.0f, 10.0f);
        return force;
    }

    Vector3 Rolling(Vector3 force){
        // Rolling a ball force
        force.x = 0.0f; //Random.Range(.0f, 20.0f);
        force.y = 0.0f;
        force.z = Random.Range(.0f, 20.0f);
        return force;
    }

    void MagnusEffect(Vector3 force3, Rigidbody rb, bool spinDirection, Vector3 direction){
        //Adding a MagnusEffect to make a ball curve by applied +-90 degree from the direction of the ball direction(perpendicular).
        //float ballCrossectionalArea = Mathf.PI * Mathf.Pow(rb.GetComponent<SphereCollider>().radius, 2);
        //float airDensity = 1.225f;   // The unit is kg/m3 from ISA-International Standard Atmosphere
        //float dragCoefficient = 10;
        //Vector3 ballVelocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        //float liftForce = 4/3 * (4 * Mathf.Pow(Mathf.PI, 2) * Mathf.Pow(rb.GetComponent<SphereCollider>().radius), 3) * airDensity * rb.velocity;
        if (spinDirection){
            //print("Spinning left");
            Vector3 magnusDirection = Quaternion.Euler(0, -90, 0) * direction;
            //rb.AddForce(Vector3.left * magnusForceWeight + Vector3.forward * magnusForceWeight, ForceMode.Impulse);
            rb.AddRelativeForce(magnusDirection * magnusForceWeight, ForceMode.Impulse);
        }
        else {
            Vector3 magnusDirection = Quaternion.Euler(0, 90, 0) * direction;
            //rb.AddForce(Vector3.left * magnusForceWeight + Vector3.forward * magnusForceWeight, ForceMode.Impulse);
            rb.AddRelativeForce(magnusDirection * magnusForceWeight, ForceMode.Impulse);
        }
    }
}