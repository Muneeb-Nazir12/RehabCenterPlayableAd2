using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }
    [SerializeField] private Text moneyText;
    private int _money = 200;
    private int _shownMoney = -1;

    private readonly StringBuilder _sb = new StringBuilder(8);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshMoney();
    }

    public bool AddMoney(int amount)
    {
        if (amount < 0 && _money + amount < 0) return false;
        _money += amount;
        RefreshMoney();
        return true;
    }

    public int GetMoney() => _money;

    private void RefreshMoney()
    {
        if (moneyText == null || _money == _shownMoney) return;
        _shownMoney = _money;
        _sb.Clear();
        _sb.Append('$').Append(_money);
        moneyText.text = _sb.ToString();
    }
}
