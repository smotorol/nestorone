using OrderInventory.Contracts.Orders;
using OrderInventory.Contracts.Products;
using OrderInventory.Client.WinForms.Services;

namespace OrderInventory.Client.WinForms.Forms;

public sealed class MainForm : Form
{
    private readonly ApiClient _apiClient = new();

    private readonly TextBox _productKeywordTextBox = new() { Width = 180 };
    private readonly Button _searchProductsButton = new() { Text = "상품 조회", Width = 90 };
    private readonly DataGridView _productsGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly TextBox _customerNameTextBox = new() { Width = 140, Text = "홍길동" };
    private readonly NumericUpDown _quantityInput = new() { Width = 80, Minimum = 1, Maximum = 999, Value = 1 };
    private readonly Button _createOrderButton = new() { Text = "주문 생성", Width = 90 };
    private readonly TextBox _createdOrderIdTextBox = new() { Width = 100, ReadOnly = true };

    private readonly TextBox _orderKeywordTextBox = new() { Width = 180 };
    private readonly ComboBox _orderStatusComboBox = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _searchOrdersButton = new() { Text = "주문 조회", Width = 90 };
    private readonly DataGridView _ordersGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    private readonly DataGridView _orderItemsGrid = new() { Dock = DockStyle.Bottom, Height = 180, ReadOnly = true, AutoGenerateColumns = true };
    private readonly TextBox _selectedOrderIdTextBox = new() { Width = 100, ReadOnly = true };
    private readonly TextBox _cancelReasonTextBox = new() { Width = 200, Text = "테스트 취소" };
    private readonly Button _cancelOrderButton = new() { Text = "주문 취소", Width = 90 };

    private readonly Label _resultLabel = new() { Dock = DockStyle.Bottom, Height = 28, Text = "결과: 대기", Padding = new Padding(8, 6, 0, 0) };

    public MainForm()
    {
        Text = "Order Inventory WinForms Client";
        Width = 1280;
        Height = 820;

        _orderStatusComboBox.Items.AddRange(["전체", "CREATED", "CANCELLED"]);
        _orderStatusComboBox.SelectedIndex = 0;

        var tabControl = new TabControl { Dock = DockStyle.Fill };
        tabControl.TabPages.Add(CreateProductsPage());
        tabControl.TabPages.Add(CreateOrdersPage());

        Controls.Add(tabControl);
        Controls.Add(_resultLabel);

        Load += async (_, _) =>
        {
            await LoadProductsAsync();
            await LoadOrdersAsync();
        };

        _searchProductsButton.Click += async (_, _) => await LoadProductsAsync();
        _createOrderButton.Click += async (_, _) => await CreateOrderAsync();
        _searchOrdersButton.Click += async (_, _) => await LoadOrdersAsync();
        _ordersGrid.SelectionChanged += async (_, _) => await LoadSelectedOrderDetailAsync();
        _cancelOrderButton.Click += async (_, _) => await CancelOrderAsync();
    }

    private TabPage CreateProductsPage()
    {
        var page = new TabPage("상품/주문 생성");

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8) };
        topPanel.Controls.AddRange(
        [
            new Label { Text = "검색어", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _productKeywordTextBox,
            _searchProductsButton
        ]);

        var bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(8) };
        bottomPanel.Controls.AddRange(
        [
            new Label { Text = "고객명", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _customerNameTextBox,
            new Label { Text = "수량", AutoSize = true, Padding = new Padding(12, 8, 0, 0) },
            _quantityInput,
            _createOrderButton,
            new Label { Text = "생성 주문ID", AutoSize = true, Padding = new Padding(12, 8, 0, 0) },
            _createdOrderIdTextBox
        ]);

        page.Controls.Add(_productsGrid);
        page.Controls.Add(topPanel);
        page.Controls.Add(bottomPanel);
        return page;
    }

    private TabPage CreateOrdersPage()
    {
        var page = new TabPage("주문 조회/취소");

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8) };
        topPanel.Controls.AddRange(
        [
            new Label { Text = "검색어", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _orderKeywordTextBox,
            new Label { Text = "상태", AutoSize = true, Padding = new Padding(12, 8, 0, 0) },
            _orderStatusComboBox,
            _searchOrdersButton
        ]);

        var bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(8) };
        bottomPanel.Controls.AddRange(
        [
            new Label { Text = "선택 주문ID", AutoSize = true, Padding = new Padding(0, 8, 0, 0) },
            _selectedOrderIdTextBox,
            new Label { Text = "취소 사유", AutoSize = true, Padding = new Padding(12, 8, 0, 0) },
            _cancelReasonTextBox,
            _cancelOrderButton
        ]);

        page.Controls.Add(_ordersGrid);
        page.Controls.Add(_orderItemsGrid);
        page.Controls.Add(topPanel);
        page.Controls.Add(bottomPanel);
        return page;
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _apiClient.GetProductsAsync(_productKeywordTextBox.Text, CancellationToken.None);
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
        if (_productsGrid.CurrentRow?.DataBoundItem is not ProductSummary selected)
        {
            _resultLabel.Text = "오류: 상품을 먼저 선택하세요.";
            return;
        }

        var request = new CreateOrderRequest
        {
            CustomerName = _customerNameTextBox.Text,
            CreatedBy = "winforms-user",
            Items =
            [
                new CreateOrderItem
                {
                    ProductId = selected.ProductId,
                    OrderQty = Decimal.ToInt32(_quantityInput.Value)
                }
            ]
        };

        try
        {
            var response = await _apiClient.CreateOrderAsync(request, CancellationToken.None);
            if (response?.Data is null)
            {
                _resultLabel.Text = response is null ? "오류: 응답이 없습니다." : $"오류: {response.Message}";
                return;
            }

            _createdOrderIdTextBox.Text = response.Data.OrderId.ToString();
            _selectedOrderIdTextBox.Text = response.Data.OrderId.ToString();
            _resultLabel.Text = $"결과: {response.Message} / 주문번호={response.Data.OrderNo}";
            await LoadProductsAsync();
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            var status = _orderStatusComboBox.SelectedItem?.ToString();
            if (status == "전체") status = null;

            var orders = await _apiClient.GetOrdersAsync(_orderKeywordTextBox.Text, status, CancellationToken.None);
            _ordersGrid.DataSource = orders.ToList();
            _resultLabel.Text = $"결과: 주문 {orders.Count}건 조회";

            if (orders.Count > 0)
            {
                _ordersGrid.Rows[0].Selected = true;
                await LoadSelectedOrderDetailAsync();
            }
            else
            {
                _selectedOrderIdTextBox.Text = string.Empty;
                _orderItemsGrid.DataSource = null;
            }
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }

    private async Task LoadSelectedOrderDetailAsync()
    {
        if (_ordersGrid.CurrentRow?.DataBoundItem is not OrderSummary selected)
        {
            return;
        }

        try
        {
            _selectedOrderIdTextBox.Text = selected.OrderId.ToString();
            var detail = await _apiClient.GetOrderByIdAsync(selected.OrderId, CancellationToken.None);
            _orderItemsGrid.DataSource = detail?.Items?.ToList();
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }

    private async Task CancelOrderAsync()
    {
        if (!long.TryParse(_selectedOrderIdTextBox.Text, out var orderId) || orderId <= 0)
        {
            _resultLabel.Text = "오류: 취소할 주문ID가 없습니다.";
            return;
        }

        var request = new CancelOrderRequest
        {
            CancelReason = string.IsNullOrWhiteSpace(_cancelReasonTextBox.Text) ? "테스트 취소" : _cancelReasonTextBox.Text,
            UpdatedBy = "winforms-user"
        };

        try
        {
            var response = await _apiClient.CancelOrderAsync(orderId, request, CancellationToken.None);
            _resultLabel.Text = response is null
                ? "오류: 응답이 없습니다."
                : $"결과: {response.Message} / 주문ID={orderId}";
            await LoadProductsAsync();
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"오류: {ex.Message}";
        }
    }
}
