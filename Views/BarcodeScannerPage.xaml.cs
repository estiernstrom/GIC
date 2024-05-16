using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;


namespace GIC.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BarcodeScannerPage : ContentPage
    {
        public BarcodeScannerPage()
        {
            InitializeComponent();

            barcodeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.Ean13,
                AutoRotate = true,
                Multiple = true
            };
        }

        private void barcodeReader_BarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();

            if (first is null)
                return;

            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Barcode Detected", first.Value, "OK");
            });
        }



    }
}