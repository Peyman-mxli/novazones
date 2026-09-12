using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public TargetFrameUI targetFrameUI;

    [Header("Targeting")]
    public float maxClickDistance = 9999f;

    private PlayerHealth playerHealth;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead())
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ClearTarget();

            return;
        }

        if (Input.GetMouseButtonDown(0))
            TrySelectTargetOrLoot();

        if (Input.GetKeyDown(KeyCode.Escape))
            ClearTarget();
    }

    private void TrySelectTargetOrLoot()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxClickDistance);

        if (hits == null || hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyTargetable target = hits[i].collider.GetComponent<EnemyTargetable>();

            if (target == null)
                target = hits[i].collider.GetComponentInParent<EnemyTargetable>();

            if (target == null)
                target = hits[i].collider.GetComponentInChildren<EnemyTargetable>();

            if (target == null)
                continue;

            if (!target.IsDead())
            {
                if (targetFrameUI != null)
                    targetFrameUI.SetTarget(target);

                return;
            }

            EnemyLoot loot = target.GetComponent<EnemyLoot>();

            if (loot == null)
                loot = target.GetComponentInParent<EnemyLoot>();

            if (loot == null)
                loot = target.GetComponentInChildren<EnemyLoot>();

            if (loot != null && loot.HasAnyLoot())
            {
                if (LootWindowUI.Instance != null)
                    LootWindowUI.Instance.OpenLootWindow(loot);
            }

            return;
        }
    }

    public void ClearTarget()
    {
        if (targetFrameUI != null)
            targetFrameUI.ClearTarget();
    }
}