using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }

    public const int XP_PER_TASK = 1;
    public const int TASKS_PER_DIAMOND = 5;
    public const int MAX_XP = 20;
    public const int START_MONEY = 200;

    [SerializeField] private Text xpText;
    [SerializeField] private Text moneyText;
    [SerializeField] private Text diamondText;

    private int _xp;
    private int _money = START_MONEY;
    private int _diamonds;
    private int _tasksCompleted;

    private int _shownXP = -1;
    private int _shownMoney = -1;
    private int _shownDiamonds = -1;

    private readonly StringBuilder _sb = new StringBuilder(8);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshXP();
        RefreshMoney();
        RefreshDiamonds();
    }

    public void OnTaskCompleted()
    {
        _tasksCompleted++;
        AddXP(XP_PER_TASK);
        if (_tasksCompleted % TASKS_PER_DIAMOND == 0)
            AddDiamonds(1);
    }

    public bool AddXP(int amount)
    {
        int next = Mathf.Clamp(_xp + amount, 0, MAX_XP);
        if (next == _xp) return true;
        _xp = next;
        RefreshXP();
        return true;
    }

    public bool AddMoney(int amount)
    {
        if (amount < 0 && _money + amount < 0) return false;
        _money += amount;
        RefreshMoney();
        return true;
    }

    public bool AddDiamonds(int amount)
    {
        if (amount < 0 && _diamonds + amount < 0) return false;
        _diamonds += amount;
        RefreshDiamonds();
        return true;
    }

    public int GetXP() => _xp;
    public int GetMoney() => _money;
    public int GetDiamonds() => _diamonds;

    private void RefreshXP()
    {
        if (xpText == null || _xp == _shownXP) return;
        _shownXP = _xp;
        _sb.Clear();
        _sb.Append(_xp).Append('/').Append(MAX_XP);
        xpText.text = _sb.ToString();
    }

    private void RefreshMoney()
    {
        if (moneyText == null || _money == _shownMoney) return;
        _shownMoney = _money;
        _sb.Clear();
        _sb.Append('$').Append(_money);
        moneyText.text = _sb.ToString();
    }

    private void RefreshDiamonds()
    {
        if (diamondText == null || _diamonds == _shownDiamonds) return;
        _shownDiamonds = _diamonds;
        _sb.Clear();
        _sb.Append(_diamonds);
        diamondText.text = _sb.ToString();
    }
}
