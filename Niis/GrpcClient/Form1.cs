using Grpc.Net.Client;
using System;
using System.Windows.Forms;
using WagonService;

namespace GrpcClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private async void submitButton_Click(object sender, EventArgs e)
        {
            string startTime = startTimeTextBox.Text;
            string endTime = endTimeTextBox.Text;
            
            if (DateTime.TryParse(startTime, out DateTime startDateTime) &&
                DateTime.TryParse(endTime, out DateTime endDateTime))
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
                    resultsDataGridView.Rows.Clear();
                    foreach (var wagon in response.Wagons)
                    {
                        resultsDataGridView.Rows.Add(wagon.InventoryNumber, wagon.ArrivalTime, wagon.DepartureTime);
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
    }
}