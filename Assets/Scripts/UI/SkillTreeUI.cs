using UnityEngine;

/// <summary>
/// Skill tree panel. Finds SkillNodeUI children placed manually in the editor
/// and binds them to the SkillTreeManager. Nodes are positioned visually
/// in the scene to form the tree layout with connecting lines.
/// </summary>
public class SkillTreeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private GameObject panelRoot;

    private SkillNodeUI[] skillNodes;

    private void Start()
    {
        if (skillTreeManager == null)
        {
            skillTreeManager = SkillTreeManager.Instance;
            if (skillTreeManager == null)
            {
                Debug.LogError("[SkillTreeUI] No SkillTreeManager found!");
                return;
            }
        }

        BindAllNodes();
    }

    /// <summary>
    /// Finds all SkillNodeUI children and binds them to the manager.
    /// </summary>
    public void BindAllNodes()
    {
        skillNodes = GetComponentsInChildren<SkillNodeUI>(true);

        foreach (var node in skillNodes)
        {
            if (node == null) continue;

            node.Bind(skillTreeManager);
            node.OnSlotClicked += Node_OnClicked;
        }
    }

    /// <summary>
    /// Unbinds all nodes and clears subscriptions.
    /// </summary>
    public void UnbindAllNodes()
    {
        if (skillNodes == null) return;

        foreach (var node in skillNodes)
        {
            if (node == null) continue;

            node.OnSlotClicked -= Node_OnClicked;
            node.Unbind();
        }
    }

    private void OnDisable()
    {
        UnbindAllNodes();
    }

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Node_OnClicked(SkillNodeUI node)
    {
        if (node.BoundSkill != null && skillTreeManager != null)
        {
            skillTreeManager.TryPurchaseSkill(node.BoundSkill);
        }
    }
}
