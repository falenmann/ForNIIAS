using Grpc.Net.Client;
using WagonService;

namespace GrpcClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeDataGridView(); 
        }
        
        private async void submitButton_Click(object sender, EventArgs e)
        {
            string startTime = startTimeTextBox.Text;
            string endTime = endTimeTextBox.Text;
            
            if (DateTime.TryParse(startTime, out var startDateTime) &&
                DateTime.TryParse(endTime, out var endDateTime))
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var channel = GrpcChannel.ForAddress("http://localhost:5012", new GrpcChannelOptions
                {
                    HttpHandler = handler
                });
                var client = new WagonsService.WagonsServiceClient(channel);

                var request = new WagonRequest
                {
                    StartTime = startTime,
                    EndTime = endTime
                };

                try
                {
                    var response = await client.GetWagonsAsync(request);
                    resultsDataGridView.Rows.Clear(); /

                    foreach (var wagon in response.Wagons)
                    {
                        
                        resultsDataGridView.Rows.Add(
                            wagon.InventoryNumber,
                            DateTime.Parse(wagon.ArrivalTime).ToString("yyyy-MM-dd HH:mm:ss"), 
                            DateTime.Parse(wagon.DepartureTime).ToString("yyyy-MM-dd HH:mm:ss")
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при запросе данных: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Некорректный формат времени. Введите корректные данные.");
            }
        }
        private void InitializeDataGridView()
        {
            resultsDataGridView.ColumnCount = 3; 
            resultsDataGridView.Columns[0].Name = "Инвентарный номер"; 
            resultsDataGridView.Columns[1].Name = "Время прибытия";    
            resultsDataGridView.Columns[2].Name = "Время отправления"; 

            resultsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; 
            resultsDataGridView.AllowUserToAddRows = false; 
            resultsDataGridView.ScrollBars = ScrollBars.Both; 
        }
    }
}