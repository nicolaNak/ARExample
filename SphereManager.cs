using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// brings in camera gameobject, finds it's position.
/// checks for existing spheres in environment
/// applies distance calculation against existing spheres and passes to shader for alpha changes
/// checks if camera is within range of sphere to speed up inner cube movements
/// </summary>

public class SphereManager : MonoBehaviour
{
    public GameObject cameraObject;
    GameObject[] sphereArray;
    List<Transform> innerCubeList;
    float innerCubeSpeed = 30f;
    
    void Start()
    {
        innerCubeList = new List<Transform>();
    }

    bool CheckCurrentSpheres()
    {
        sphereArray = GameObject.FindGameObjectsWithTag("Sphere");
        if (sphereArray.Length <= 0) { return false; }
        else
        {
            foreach (GameObject sphere in sphereArray)
            {
                //get the size of the sphere - each sphere is the same for now but this way allows for changes to the mesh size/scale of the object being spawned
                //will assume every mesh used is based on a sphere for ease of distance calculations
                float sphereSize = sphere.GetComponent<MeshFilter>().mesh.bounds.extents.x;
                //get the distance from the centre of the sphere
                float sphereDistance = Vector3.Distance(sphere.transform.position, cameraObject.transform.position);
                //apply sphere size to make sure transparency effect works correctly ie starts to fade before the player reaches the mesh, if the sphere was a scale of 1:1
                sphereDistance -= sphereSize;
                //before applying to alpha, check if at edge of sphere itself and hide mesh if true
                MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
                if (sphereDistance <= sphereSize) { renderer.enabled = false; }
                else { renderer.enabled = true; }
                //ramp up the transparency exponentially the closer to the sphere the user is
                float sphereAlpha = Mathf.Pow(sphereDistance, 3f);
                if (sphereAlpha > 1f) sphereAlpha = 1f;
                //apply the distance to the material colour
                Color sphereColour = sphere.GetComponent<MeshRenderer>().material.color;
                sphereColour.a = sphereAlpha;
                sphere.GetComponent<MeshRenderer>().material.color = sphereColour;
                //get reference to inner cube
                Transform currentCube = sphere.transform.GetChild(0);
                if (innerCubeList.Count > 0)
                {
                    for (int c = 0; c < innerCubeList.Count; c++)
                    {
                        if (innerCubeList[c].position != currentCube.position)
                        {
                            if(c == innerCubeList.Count - 1)
                            {
                                innerCubeList.Add(currentCube);
                            }
                        }
                    }
                }
                else
                {
                    //no cubes to check in list so can just add to it
                    innerCubeList.Add(currentCube);
                }
            }
            return true;
        }
    }

    //apply movement to inner cubes that can be viewed when move closer to the spheres and speeds up the closer you get 
    void InnerCubeMovement()
    {
        //this assumes the sphere and cube arrays have the same order, will need refactoring if spheres can be destroyed
        for(int i = 0; i < innerCubeList.Count; i++)
        {
            Vector3 sphereCentre = sphereArray[i].transform.position;
            //find distance of camera from cube, since cube is very small not going to worry about size adjustments
            float cubeDistance = Vector3.Distance(cameraObject.transform.position, innerCubeList[i].position);
            //similar to what is used to affect the alpha on the sphere
            float cubeExponentialDistance = Mathf.Pow(5f - cubeDistance, 2);
            //apply distance to speed to increase it the closer the user is to this cube instance
            float correctedSpeed = innerCubeSpeed * cubeExponentialDistance;
            innerCubeList[i].RotateAround(sphereCentre, new Vector3(1f,1f,0f), correctedSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (CheckCurrentSpheres()) { InnerCubeMovement(); }
    }
}
