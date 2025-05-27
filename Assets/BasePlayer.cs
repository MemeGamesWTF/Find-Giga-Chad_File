using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BasePlayer : MonoBehaviour
{

    private bool isDraggingCamera = false;
    private Vector2 lastDragPosition;

    public float moveSpeed = 5f;

    // Boundaries for left and right movement
    public float leftBoundary = -4f;
    public float rightBoundary = 5f;

    public float cameraLeftBoundary = -10f;  // Set these values as needed.
    public float cameraRightBoundary = 10f;

    public float cameraMoveSpeed = 1f;

    void Start()
    {

    }

    void Update()
    {
        if (!GameManager.Instance.GameState)
            return;

        // --- Handle Mouse Input ---
        if (Input.mousePresent)
        {
            // When the mouse button is first pressed
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
                bool hitAldo = false;
                // Check for aldo first.
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("aldo"))
                    {
                        GameManager.Instance.tappedAldoCount++;
                        GameManager.Instance.Tap.Play();
                        hit.collider.gameObject.SetActive(false);
                        GameObject foodObject = hit.collider.gameObject;
                        Vector3 collisionPosition = foodObject.gameObject.transform.position;
                        //  Debug.Log("aldo");
                        GameManager.Instance.poof.transform.position = collisionPosition;
                        GameManager.Instance.poof.Play();
                        hitAldo = true;
                        GameManager.Instance.collisionCount++;
                        GameManager.Instance.AddScore();
                        if (GameManager.Instance.collisionCount <= GameManager.Instance.hearts.Length)
                        {
                            GameObject heart = GameManager.Instance.hearts[GameManager.Instance.collisionCount - 1];

                            // Find the Particle System inside the heart
                            ParticleSystem heartParticle = heart.GetComponentInChildren<ParticleSystem>();

                            if (heartParticle != null)
                            {
                                heartParticle.Play(); // Play the particle effect
                            }

                            // Wait for the particle effect to finish before deactivating the heart
                            GameManager.Instance.StartCoroutine(DisableHeartAfterEffect(heart, heartParticle));
                        }

                        break;
                    }
                }
               

               

                if (GameManager.Instance.tappedAldoCount >= 3 && GameManager.Instance.progressBar.value > 0)
                {
                    GameManager.Instance.GameWin();
                    GameManager.Instance.collisionCount = 0;
                }
                // If no aldo was hit, check for Background.
                if (!hitAldo)
                {
                    bool hitBackground = false;
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider.CompareTag("Background"))
                        {
                            isDraggingCamera = true;
                            lastDragPosition = worldPos;
                            hitBackground = true;
                            break;
                        }
                    }
                    // If neither aldo nor background was hit, move the player.
                    if (!hitBackground)
                    {
                        MovePlayer(worldPos.x);
                    }
                }
            }
            // While the mouse button is held down
            if (Input.GetMouseButton(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (isDraggingCamera)
                {
                    // Calculate the movement delta
                    float deltaX = worldPos.x - lastDragPosition.x;

                    // Update and clamp the camera's position.
                    Vector3 newCamPos = Camera.main.transform.position;
                    newCamPos.x -= deltaX * cameraMoveSpeed;
                    newCamPos.x = Mathf.Clamp(newCamPos.x, cameraLeftBoundary, cameraRightBoundary);
                    Camera.main.transform.position = newCamPos;
                    lastDragPosition = worldPos;
                }
                else
                {
                    MovePlayer(worldPos.x);
                }
            }
            // When the mouse button is released, stop dragging the camera.
            if (Input.GetMouseButtonUp(0))
            {
                isDraggingCamera = false;
            }
        }

        // --- Handle Touch Input ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
                bool hitAldo = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("aldo"))
                    {
                        GameManager.Instance.Tap.Play();
                        GameManager.Instance.tappedAldoCount++;
                        hit.collider.gameObject.SetActive(false);
                        GameManager.Instance.AddScore();
                        GameObject foodObject = hit.collider.gameObject;
                        Vector3 collisionPosition = foodObject.gameObject.transform.position;
                        //  Debug.Log("aldo");
                        GameManager.Instance.poof.transform.position = collisionPosition;
                        GameManager.Instance.poof.Play();
                        GameManager.Instance.collisionCount++;

                        if (GameManager.Instance.collisionCount <= GameManager.Instance.hearts.Length)
                        {
                            GameObject heart = GameManager.Instance.hearts[GameManager.Instance.collisionCount - 1];

                            // Find the Particle System inside the heart
                            ParticleSystem heartParticle = heart.GetComponentInChildren<ParticleSystem>();

                            if (heartParticle != null)
                            {
                                heartParticle.Play(); // Play the particle effect
                            }

                            // Wait for the particle effect to finish before deactivating the heart
                            GameManager.Instance.StartCoroutine(DisableHeartAfterEffect(heart, heartParticle));
                        }

                        //  Debug.Log("aldo");
                        hitAldo = true;
                        break;
                    }
                }
               

                

                if (GameManager.Instance.tappedAldoCount >= 3 && GameManager.Instance.progressBar.value > 0)
                {
                    GameManager.Instance.GameWin();
                    GameManager.Instance.collisionCount = 0;
                }
                if (!hitAldo)
                {
                    bool hitBackground = false;
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider != null && hit.collider.CompareTag("Background"))
                        {
                            isDraggingCamera = true;
                            lastDragPosition = worldPos;
                            hitBackground = true;
                            break;
                        }
                    }
                    if (!hitBackground)
                    {
                        MovePlayer(worldPos.x);
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (isDraggingCamera)
                {
                    float deltaX = worldPos.x - lastDragPosition.x;
                    Vector3 newCamPos = Camera.main.transform.position;
                    newCamPos.x -= deltaX * cameraMoveSpeed;
                    newCamPos.x = Mathf.Clamp(newCamPos.x, cameraLeftBoundary, cameraRightBoundary);
                    Camera.main.transform.position = newCamPos;
                    lastDragPosition = worldPos;
                }
                else
                {
                    MovePlayer(worldPos.x);
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDraggingCamera = false;
            }
        }

        // Additional update logic can go here.
    }

    private void MovePlayer(float targetX)
    {
        // Clamp the target x value within the defined boundaries.
        float clampedX = Mathf.Clamp(targetX, leftBoundary, rightBoundary);
        Vector3 targetPosition = new Vector3(clampedX, transform.position.y, transform.position.z);
        // Gradually move towards the target position based on the moveSpeed.
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }


    public void GameOver()
    {
        GameManager.Instance.GameOVer();
    }

    public void Reset()
    {

    }

    private IEnumerator DisableHeartAfterEffect(GameObject heart, ParticleSystem particle)
    {
        if (particle != null)
        {
            yield return new WaitForSeconds(particle.main.duration); // Wait for particle effect to finish
        }

        heart.SetActive(false); // Now deactivate the heart
    }

}
