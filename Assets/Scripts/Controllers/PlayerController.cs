using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public BlockScriptableObject block;

    public World world;

    public new CameraController camera;

    public float speed = 5.0f;
    public float jumpForce = 50.0f;

    public float breakDistance = 5.0f;

    public LayerMask chunkMeshMask;

    [Range(0, 0.3f)]
    public float movementSmoothing = 0.05f;

    private new Rigidbody rigidbody;
    private Vector3 velocity = new Vector3();

    void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    }

    void Update()
    {

        SetBlock();



    }

    void FixedUpdate()
    {
        Fly();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward * this.breakDistance);
    }




    public void Fly()
    {
        this.transform.eulerAngles = new Vector3(0, camera.yaw, 0);

        this.velocity = this.transform.forward * Input.GetAxisRaw("Vertical");
        this.velocity += this.transform.right * Input.GetAxisRaw("Horizontal");
        this.velocity += Vector3.up * (Input.GetAxisRaw("Jump") - Input.GetAxisRaw("Duck"));
        this.velocity *= this.speed * Time.deltaTime * 10.0f;

        var targetVelocity = velocity;

        this.rigidbody.velocity = Vector3.SmoothDamp(this.rigidbody.velocity, targetVelocity, ref targetVelocity, this.movementSmoothing);

    }



    public void SetBlock()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Vector3Int blockCoord = this.world.WorldCoordinateToBlock(camera.transform.position);
            Vector3Int chunkCoord = this.world.WorldCoordinateToChunk(camera.transform.position);

            Chunk currentChunk = world.chunks[chunkCoord];

            Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit);

            if (hit.collider == null)
            {
                return;
            }

            Chunk chunk = hit.collider.GetComponent<Chunk>();

            Vector3 coordinates = new Vector3();
            Vector3Int localCoordinatesInt = new Vector3Int();
            Vector3Int globalCoordinatesInt = new Vector3Int();



            if (Input.GetMouseButtonDown(0))
            {
                coordinates = hit.point + -hit.normal / 2;

                int x = Mathf.RoundToInt(coordinates.x) % 16;
                int y = Mathf.RoundToInt(coordinates.y) % 16;
                int z = Mathf.RoundToInt(coordinates.z) % 16;

                localCoordinatesInt.x = x; localCoordinatesInt.y = y; localCoordinatesInt.z = z;

                chunk.BreakBlock(localCoordinatesInt);
            }
            if (Input.GetMouseButtonDown(1))
            {
                


                coordinates = hit.point + hit.normal / 2;

                int gX = Mathf.RoundToInt(coordinates.x);
                int gY = Mathf.RoundToInt(coordinates.y);
                int gZ = Mathf.RoundToInt(coordinates.z);

                globalCoordinatesInt.x = gX; globalCoordinatesInt.y = gY; globalCoordinatesInt.z = gZ;

                int x = Mathf.RoundToInt(coordinates.x) % 16;
                int y = Mathf.RoundToInt(coordinates.y) % 16;
                int z = Mathf.RoundToInt(coordinates.z) % 16;

                localCoordinatesInt.x = x; localCoordinatesInt.y = y; localCoordinatesInt.z = z;

                world.GetChunkByGlobalBlockCoordinates(globalCoordinatesInt).ResetBlock(localCoordinatesInt, block);

                //chunk.ResetBlock(localCoordinatesInt, block);
            }
        }
    }




    public void BreackBlock()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var blockCoordinates = this.world.WorldCoordinateToBlock(camera.transform.position);
            var chunkCoordinates = this.world.WorldCoordinateToChunk(camera.transform.position);

            var currentChunk = this.world.chunks[chunkCoordinates];

            Debug.Log(blockCoordinates);

            if (currentChunk.blocks[blockCoordinates.x, blockCoordinates.y, blockCoordinates.z] != null)
            {
                currentChunk.BreakBlock(blockCoordinates);
            }
            else if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit, this.breakDistance, this.chunkMeshMask))
            {
                var chunk = hit.collider.GetComponent<Chunk>();

                var coordinates = hit.point + -hit.normal * 0.5f;

                var x = Mathf.RoundToInt(coordinates.x) % 16;
                var y = Mathf.RoundToInt(coordinates.y) % 16;
                var z = Mathf.RoundToInt(coordinates.z) % 16;

                chunk.BreakBlock(new Vector3Int(x, y, z));
            }
        }
    }
}
