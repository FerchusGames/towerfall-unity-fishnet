using FishNet.Object;
using FishNet.Observing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PredictionManager : MonoBehaviour
{
    // This is for 2D, to turn it into 3D, just remove all "2D" from the code.
    
    Scene currentScene;
    Scene predictionScene;

    PhysicsScene2D currentPhysicsScene;
    PhysicsScene2D predictionPhysicsScene;

    void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;

        currentScene = SceneManager.GetActiveScene();
        currentPhysicsScene = currentScene.GetPhysicsScene2D();

        // Create prediction scene
        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        predictionScene = SceneManager.CreateScene("PredictionScene", parameters);
        predictionPhysicsScene = predictionScene.GetPhysicsScene2D();
    }

    void FixedUpdate()
    {
        if (currentPhysicsScene.IsValid())
        {
            currentPhysicsScene.Simulate(Time.fixedDeltaTime); // The main scene's physics will work normally
        }
    }

    GameObject AddDummyObjectToPhysicsScene(GameObject _go)
    {
        GameObject dummyGo = Instantiate(_go); // Copying the object we want to predict
        /*dummyGo.transform.position = _go.transform.position; // This is only needed in old editor scenes
        dummyGo.transform.rotation = _go.transform.rotation;*/
        Renderer fakeRender = dummyGo.GetComponent<Renderer>();
        if (fakeRender)
        {
            fakeRender.enabled = false;
        }

        SceneManager.MoveGameObjectToScene(dummyGo, predictionScene);
        return dummyGo;
    }

    public (Vector2, Vector2) Predict(GameObject _go, Vector2 _velocity, int _steps)
    {
        if (!currentPhysicsScene.IsValid() || !predictionPhysicsScene.IsValid())
        {
            Debug.LogError("Una escena no es valida");
        }

        GameObject dummy = AddDummyObjectToPhysicsScene(_go);
        Destroy(dummy.GetComponent<NetworkObserver>());
        Destroy(dummy.GetComponent<NetworkObject>());
        dummy.SetActive(true); // Reactivate because NetworkObject disables it when cloning.
        Rigidbody2D dummyRigid2d = dummy.GetComponent<Rigidbody2D>();
        dummyRigid2d.velocity = _velocity;

        for (int i = 0; i <= _steps; i++)
        {
            predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
            // dummyRigid2d.position // CUrrent position of this step
        }

        Vector2 lastPosition = dummyRigid2d.position;
        Vector2 lastVelocity = dummyRigid2d.velocity;
        Destroy(dummy);
        return (lastPosition, lastVelocity);
    }
}