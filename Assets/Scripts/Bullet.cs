using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Block target;
    private Shooter shooter;
    public float speed = 10f;
    private bool hasHit = false;
    private bool isDestroyed = false; // Thêm flag để tránh xử lý nhiều lần

    public void Init(Block targetBlock, Shooter shooterSource)
    {
        target = targetBlock;
        shooter = shooterSource;

        // Đảm bảo bullet có Rigidbody và Collider
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>().useGravity = false;
        }

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<SphereCollider>().isTrigger = true;
        }

        // Chạy về phía mục tiêu
        if (target != null)
        {
            StartCoroutine(MoveToTarget());
        }
        else
        {
            DestroyBulletSafely();
        }
    }

    private IEnumerator MoveToTarget()
    {
        while (target != null && !hasHit && !isDestroyed)
        {
            // Kiểm tra target còn tồn tại và có floor > 0
            if (target.gameObject == null || target.floor <= 0)
            {
                Debug.Log("Target block no longer exists or has no floor, destroying bullet");
                DestroyBulletSafely();
                yield break;
            }

            Vector3 direction = (target.transform.position - transform.position).normalized;

            // Di chuyển bullet theo hướng target
            transform.position += direction * speed * Time.deltaTime;

            // Kiểm tra khoảng cách để tránh bullet bay quá xa
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget > 50f)
            {
                Debug.Log("Bullet traveled too far, destroying");
                DestroyBulletSafely();
                yield break;
            }

            // Kiểm tra khoảng cách gần để trigger hit
            if (distanceToTarget < 0.5f && !hasHit)
            {
                TryHitTarget();
                yield break;
            }

            yield return null;
        }

        // Nếu không hit được, tự hủy
        if (!hasHit && !isDestroyed)
        {
            DestroyBulletSafely();
        }
    }

    private void TryHitTarget()
    {
        if (hasHit || isDestroyed) return;
        if (target == null || target.gameObject == null) return;
        if (target.floor <= 0) return; // Block đã hết floor

        hasHit = true;
        Debug.Log($"Bullet hitting target block with floor: {target.floor}");

        // Lock target để tránh race condition
        bool hitSuccess = target.TryHit(); // Cần implement method này trong Block.cs

        if (hitSuccess)
        {
            // Trừ đạn ở shooter
            if (shooter != null)
            {
                shooter.ReduceAmmo();
            }

            // Kiểm tra và xử lý khi block bị hủy
            if (target.floor <= 0)
            {
                HandleBlockDestroyed();
            }
        }
        else
        {
            Debug.Log("Hit failed - target already being processed");
        }

        DestroyBulletSafely();
    }

    private void HandleBlockDestroyed()
    {
        Debug.Log($"Block destroyed by bullet!");

        // Tìm vị trí block trong grid
        BlockGridManager blockGrid = FindObjectOfType<BlockGridManager>();
        if (blockGrid != null && target != null && target.gameObject != null)
        {
            // Tìm vị trí grid
            for (int x = 0; x < blockGrid.width; x++)
            {
                for (int y = 0; y < blockGrid.height; y++)
                {
                    if (blockGrid.GetObjectAt(x, y) == target.transform)
                    {
                        blockGrid.ClearCell(x, y);

                        // Destroy block object
                        if (target.gameObject != null)
                        {
                            Destroy(target.gameObject);
                        }

                        blockGrid.DropColumnDown(x, y);

                        // Thông báo GameController
                        GameController gameController = FindObjectOfType<GameController>();
                        if (gameController != null)
                        {
                            gameController.RecheckAllSelectedShooters();
                        }
                        return;
                    }
                }
            }
        }
    }

    private void DestroyBulletSafely()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || isDestroyed) return;
        if (!other.CompareTag("Block")) return;

        Block hitBlock = other.GetComponent<Block>();
        if (hitBlock != null && hitBlock == target && hitBlock.floor > 0)
        {
            TryHitTarget();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit || isDestroyed) return;
        if (!collision.gameObject.CompareTag("Block")) return;

        Block hitBlock = collision.gameObject.GetComponent<Block>();
        if (hitBlock != null && hitBlock == target && hitBlock.floor > 0)
        {
            TryHitTarget();
        }
    }
}