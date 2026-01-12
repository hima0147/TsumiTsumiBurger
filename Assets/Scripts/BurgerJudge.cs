using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurgerJudge : MonoBehaviour
{
    // ���ǉ��F�����G�t�F�N�g�̃v���n�u������g
    [Header("���o")]
    [SerializeField] private GameObject explosionPrefab;

    private bool isJudged = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJudged) return;

        // ��ނ����o���Y�ɓ�����������������
        if (!collision.gameObject.CompareTag("Ingredient") &&
            !collision.gameObject.CompareTag("BottomBun")) return;

        CheckBurger();
    }

    private void CheckBurger()
    {
        Vector2 startPos = new Vector2(transform.position.x, transform.position.y - 0.1f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, Vector2.down, 10.0f);

        bool hasBottomBun = false;
        List<GameObject> burgerParts = new List<GameObject>();

        burgerParts.Add(this.gameObject);

        foreach (RaycastHit2D hit in hits)
        {
            GameObject target = hit.collider.gameObject;

            if (target == this.gameObject) continue;

            if (target.CompareTag("Ingredient"))
            {
                burgerParts.Add(target);
            }
            else if (target.CompareTag("BottomBun"))
            {
                hasBottomBun = true;
                burgerParts.Add(target);
                break;
            }
            else if (target.CompareTag("TopBun"))
            {
                break;
            }
        }

        if (hasBottomBun)
        {
            CompleteBurger(burgerParts);
        }
    }

    private void CompleteBurger(List<GameObject> parts)
    {
        isJudged = true;
        GameManager.Instance.AddScore(100 * parts.Count, parts);

        // ���C���F���o�R���[�`�����J�n
        StartCoroutine(AnimateAndDestroy(parts));
    }

    private IEnumerator AnimateAndDestroy(List<GameObject> parts)
    {
        // 1. �y�d�v�z�����ɓ����蔻��������āu�H��v�ɂ���
        // ����ŁA�ォ�痎���Ă�����ނ͂��蔲���Ă���
        foreach (GameObject part in parts)
        {
            if (part == null) continue;

            // �����蔻�������
            Collider2D col = part.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // �d�͂�؂��Ă��̏�ɌŒ�i�����Ȃ��悤�ɂ���j
            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // �F���s�J�b�ƌ��点��
            SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = new Color(1f, 1f, 0.5f, 1f); // ���F
        }

        // 2. �h��Ȕ����G�t�F�N�g�𐶐��I
        if (explosionPrefab != null)
        {
            // ��o���Y�̈ʒu�Ƀh�J���Əo��
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 3. ��u�~�܂��Č�����i0.5�b�j
        yield return new WaitForSeconds(0.5f);

        // 4. �X�D�[�b�Ə�����i�t�F�[�h�A�E�g�j
        float fadeDuration = 0.5f;
        float currentTime = 0f;

        // ���̐F�i���点���F�j���擾
        Color startColor = new Color(1f, 1f, 0.5f, 1f);
        Color endColor = new Color(1f, 1f, 0.5f, 0f); // ����

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / fadeDuration;

            foreach (GameObject part in parts)
            {
                if (part == null) continue;
                SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.Lerp(startColor, endColor, t);
                }
            }
            yield return null;
        }

        // 5. �폜�ƕ�[
        int laneIndex = GetLaneIndexFromX(transform.position.x);
        foreach (GameObject part in parts)
        {
            Destroy(part);
        }
        GameManager.Instance.RefillLane(laneIndex);
    }

    private int GetLaneIndexFromX(float xPos)
    {
        if (xPos < -1.35f) return 0;
        if (xPos < 0f) return 1;
        if (xPos < 1.35f) return 2;
        return 3;
    }
}