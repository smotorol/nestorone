using OrderInventory.Client.WinForms.Models;
using OrderInventory.Client.WinForms.Services;

namespace OrderInventory.Client.WinForms.Forms;

public sealed class MainForm : Form
{
    private readonly ApiClient _apiClient = new();
    private readonly TextBox _keywordTextBox = new() { Width = 220 };
    private readonly Button _searchButton = new() { Text = "상품 조회", Width = 100 };
    private readonly DataGridView _productsGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly TextBox _customerNameTextBox = new() { Width = 220, Text = "홍길동" };
    private readonly NumericUpDown _quantityInput = new() { Width = 120, Minimum = 1, Maximum = 999, Value = 1 };
    private readonly Button _createOrderButton = new() { Text = "주문 생성", Width = 100 };
    private readonly Label _resultLabel = new() { AutoSize = true, Text = "결과: 대기" };

    public MainForm()
    {
        Text = "Order Inventory WinForms Client";
        Width = 1100;
        Height = 700;

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8) };
        topPanel.Controls.AddRange(new Control[]
        {
            new Label { Text = "검색어", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _keywordTextBox,
            _searchButton
        });

        var bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(8) };
        bottomPanel.Controls.AddRange(new Control[]
        {
            new Label { Text = "고객명", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _customerNameTextBox,
            new Label { Text = "수량", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _quantityInput,
            _createOrderButton,
            _resultLabel
        });

        Controls.Add(_productsGrid);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        Load += async (_, _) => await LoadProductsAsync();
        _searchButton.Click += async (_, _) => await LoadProductsAsync();
        _createOrderButton.Click += async (_, _) => await CreateOrderAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _apiClient.GetProductsAsync(_keywordTextBox.Text, CancellationToken.None);
            _productsGrid.DataSource = products.ToList();
            _resultLabel.Text = $"결과: 상품 {products.Count}건 조회";
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }

    private async Task CreateOrderAsync()
    {
        if (_productsGrid.CurrentRow?.DataBoundItem is not ProductViewModel selected)
        {
            _resultLabel.Text = "오류: 상품을 먼저 선택하세요.";
            return;
        }

        var request = new CreateOrderRequest
        {
            CustomerName = _customerNameTextBox.Text,
            Items = new List<CreateOrderItemRequest>
            {
                new()
                {
                    ProductId = selected.ProductId,
                    OrderQty = Decimal.ToInt32(_quantityInput.Value)
                }
            }
        };

        try
        {
            var response = await _apiClient.CreateOrderAsync(request, CancellationToken.None);
            _resultLabel.Text = response is null
                ? "오류: 응답이 없습니다."
                : $"결과: {response.Message} / 주문번호={response.Data?.OrderNo}";
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }
}
