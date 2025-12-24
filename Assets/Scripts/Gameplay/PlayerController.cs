using UnityEngine;
using mash2.Core;

namespace mash2.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        
        [Header("Shooting")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.2f; // Выстрелов в секунду
        
        private Rigidbody2D rb;
        private Camera mainCamera;
        
        private Vector2 moveInput;
        private Vector2 mouseWorldPos;
        
        private float nextFireTime = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
            
            // Если firePoint не назначен, стреляем из центра игрока
            if (firePoint == null)
                firePoint = transform;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                moveInput = Vector2.zero;
                return;
            }
            
            // Движение
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;
            
            // Мышь
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            
            // Стрельба
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = moveInput * moveSpeed;
            RotateTowardsMouse();
        }

        private void RotateTowardsMouse()
        {
            Vector2 direction = mouseWorldPos - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        /// <summary>
        /// Стреляет пулей
        /// </summary>
        private void Shoot()
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("Bullet prefab not assigned!");
                return;
            }
    
        // Звук выстрела
            if (mash2.Audio.AudioManager.Instance != null)
            {
                mash2.Audio.AudioManager.Instance.PlayShootSound();
            }
    
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Launch(GetAimDirection());
            }
        }

        public Vector2 GetAimDirection()
        {
            return (mouseWorldPos - (Vector2)transform.position).normalized;
        }
    }
}