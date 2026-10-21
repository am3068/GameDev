using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryHotbarUI : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Button[] slotButtons = new Button[10];

    [SerializeField] private Color normalColour =
        new Color(0.22f, 0.24f, 0.28f, 1f);

    [SerializeField] private Color selectedColour =
        new Color(0.85f, 0.68f, 0.25f, 1f);

    private TMP_Text[] labels;
    private InputAction[] slotKeys = new InputAction[10];
    private InputAction useKey;
    private int selectedSlot = 0;

    private void Awake()
    {
        labels = new TMP_Text[slotButtons.Length];

        if (inventory == null && playerInput != null)
            inventory = playerInput.GetComponent<InventorySystem>();

        if (playerInput != null)
        {
            for (int i = 0; i < 10; i++)
            {
                string actionName = i == 9
                    ? "Player/Hotbar0"
                    : $"Player/Hotbar{i + 1}";

                slotKeys[i] =
                    playerInput.actions.FindAction(actionName, false);
            }

            useKey = playerInput.actions.FindAction(
                "Player/UseItem", false);
        }

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
                continue;

            int index = i;

            labels[i] =
                slotButtons[i].GetComponentInChildren<TMP_Text>();

            slotButtons[i].onClick.AddListener(
                () => SelectSlot(index));

            slotButtons[i].transition =
                Selectable.Transition.None;
        }
    }

    private void Update()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (slotKeys[i] != null &&
                slotKeys[i].WasPressedThisFrame())
            {
                SelectSlot(i);
            }
        }

        if (useKey != null &&
            useKey.WasPressedThisFrame() &&
            inventory != null)
        {
            inventory.UseItemInSlot(selectedSlot);
        }

        UpdateLabels();
    }

    private void SelectSlot(int slot)
    {
        selectedSlot = slot;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null ||
                slotButtons[i].image == null)
                continue;

            slotButtons[i].image.color =
                i == selectedSlot
                    ? selectedColour
                    : normalColour;
        }
    }

    private void UpdateLabels()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] == null)
                continue;

            string number = i == 9 ? "0" : (i + 1).ToString();

            if (inventory != null &&
                i < inventory.GetSlotCount())
            {
                labels[i].text =
                    $"{number}\n{inventory.GetItemName(i)}: " +
                    inventory.GetItemCount(i);
            }
            else
            {
                labels[i].text = $"{number}\nEmpty";
            }
        }
    }

    private void Start()
    {
        UpdateSelection();
        UpdateLabels();
    }
}