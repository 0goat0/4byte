using TMPro;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("RTS Selection System Connection")]
    [SerializeField] private PlayerInteractionState playerInteractionState; // 유닛 선택 매니저 등록

    [Header("Data Asset (Runtime Preview)")]
    [SerializeField] private PlayerData playerData;

    [Header("UI Text Components")]
    [SerializeField] private TextMeshProUGUI unitInfoText;

    [Header("Upgrade")]
    [SerializeField] private Button attackUpButton;
    [SerializeField] private Button defenseUpButton;

    private PlayerStats _trackedUnit;
    private PlayerData _cachedData;
    private EngineeringBay _currentLab;

    private void Start()
    {
        // 버튼 이벤트 리스너 등록
        if (attackUpButton != null) attackUpButton.onClick.AddListener(OnAttackUpClicked);
        if (defenseUpButton != null) defenseUpButton.onClick.AddListener(OnDefenseUpClicked);

        ClearUI();
    }

    private void OnEnable()
    {
        if (playerInteractionState != null)
        {
            // 선택 변경 이벤트 구독
            playerInteractionState.OnSelectionChanged += HandleSelectionChanged;
        }
    }

    private void OnDisable()
    {
        if (playerInteractionState != null)
        {
            playerInteractionState.OnSelectionChanged -= HandleSelectionChanged;
        }
    }

    private void Update()
    {
        // 실시간 체력 및 스탯 갱신 (유닛을 추적 중일 때만 실행)
        if (_trackedUnit != null && _trackedUnit.Object != null && _trackedUnit.Object.IsValid)
        {
            UpdateUnitInfoUI();
        }
        else if (_trackedUnit != null)
        {
            ClearUI();
        }
    }

    // PlayerStats 또는 EngineeringBay 추출
    private void HandleSelectionChanged()
    {
        if (playerInteractionState == null) return;

        // 유니티 컴포넌트 존재 검사
        if (playerInteractionState.InfoTarget is Component targetComponent)
        {
            // 엔지니어링베이 인지 확인
            EngineeringBay selectedLab = targetComponent.GetComponentInParent<EngineeringBay>();
            if (selectedLab != null)
            {
                _trackedUnit = null;
                _cachedData = null;
                _currentLab = selectedLab;

                UpdateBuildingInfoUI();
                SetUpgradeButtonsActive(true); // 업그레이드 버튼 활성화
                return;
            }

            //플레이어 유닛 확인
            PlayerStats selectedUnit = targetComponent.GetComponentInParent<PlayerStats>();
            if (selectedUnit != null)
            {
                _currentLab = null;
                SetUpgradeButtonsActive(false); // 건물 전용 버튼 숨기기

                _trackedUnit = selectedUnit;

                // PlayerStats private data 가져옴
                var fieldInfo = typeof(PlayerStats).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    _cachedData = fieldInfo.GetValue(_trackedUnit) as PlayerData;
                    if (_cachedData != null)
                    {
                        UpdateUnitInfoUI();
                        return;
                    }
                }
            }
        }

        ClearUI();
    }

    private void ClearUI()
    {
        _trackedUnit = null;
        _cachedData = null;
        _currentLab = null;

        if (unitInfoText != null)
        {
            unitInfoText.text = "";
        }

        SetUpgradeButtonsActive(false);
    }

    public void UpdateUnitInfoUI()
    {
        if (_trackedUnit == null || _cachedData == null || unitInfoText == null) return;

        string sizeStr = _cachedData.size switch
        {
            PlayerSize.Small => "Small",
            PlayerSize.Medium => "Medium",
            PlayerSize.Large => "Large",
            _ => _cachedData.size.ToString()
        };

        string attackTypeStr = _cachedData.attackType switch
        {
            PlayerAttackType.Melee => "Melee",
            PlayerAttackType.Ranged => "Ranged",
            PlayerAttackType.AoE => "AoE",
            _ => _cachedData.attackType.ToString()
        };

        unitInfoText.text = $"<line-height=85%><size=150%><b>{_cachedData.PlayerName}</b></size>\n\n" +
                             $"Size: {sizeStr}<pos=45%>Attack Type: {attackTypeStr}\n\n" +
                             $"HP: {(int)_trackedUnit.CurrentHp} / {(int)_cachedData.hp}<pos=45%>Defense: {_trackedUnit.defense}\n\n" +
                             $"Attack: {_trackedUnit.attackDamage}<pos=45%>AttackSpeed: {_trackedUnit.attackSpeed}\n\n" +
                             $"MoveSpeed: {_trackedUnit.MoveSpeed}</line-height>";
    }

    private void UpdateBuildingInfoUI()
    {
        if (_currentLab == null || unitInfoText == null) return;

        BuildingData labData = _currentLab.LabData;
        if (labData == null) return;

        int maxLabHp = labData.hp;
        int labDefense = labData.defense;

        // 출력
        unitInfoText.text = $"<line-height=85%><size=150%><b>{labData.buildingName}</b></size>\n\n" +
                             $"HP: {maxLabHp} / {maxLabHp}<pos=45%>Defense: {labDefense}</line-height>";

    }

    private void SetUpgradeButtonsActive(bool isActive)
    {
        if (attackUpButton != null) attackUpButton.gameObject.SetActive(isActive);
        if (defenseUpButton != null) defenseUpButton.gameObject.SetActive(isActive);
    }

    // 공격력 업그레이드
    private void OnAttackUpClicked()
    {
        if (_currentLab == null || playerData == null) return;

        // 베이스 공격 스텟 상승
        playerData.attack += 5; // 증가 수치

        if (_trackedUnit != null)
        {
            _trackedUnit.attackDamage += 5;
        }

        Debug.Log($"공격력 연구 완료 현재: {playerData.attack}");

        // UI 즉시 갱신
        if (_trackedUnit != null) UpdateUnitInfoUI();
        else UpdateBuildingInfoUI();
    }

    private void OnDefenseUpClicked()
    {
        if (_currentLab == null || playerData == null) return;

        playerData.defense += 2;

        if (_trackedUnit != null)
        {
            _trackedUnit.defense += 2;
        }

        Debug.Log($"방어력 연구 완료 현재: {playerData.defense}");

        // UI 갱신
        if (_trackedUnit != null) UpdateUnitInfoUI();
        else UpdateBuildingInfoUI();
    }
}