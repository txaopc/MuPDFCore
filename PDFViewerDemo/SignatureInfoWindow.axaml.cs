using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MuPDFCore.MuPDFRenderer;

namespace PDFViewerDemo
{
    public partial class SignatureInfoWindow : Window
    {
        public SignatureInfoWindow()
        {
            InitializeComponent();
        }

        public SignatureInfoWindow(SignatureField signatureField)
        {
            InitializeComponent();
            PopulateFields(signatureField);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void PopulateFields(SignatureField sig)
        {
            if (sig == null)
                return;

            this.FindControl<TextBlock>("HeaderText").Text = !string.IsNullOrEmpty(sig.SignerName)
                ? $"Digital Signature — {sig.SignerName}"
                : "Digital Signature";

            SetText("FieldNameText", sig.FieldName);
            SetText("SignerNameText", sig.SignerName);
            SetText("SigningTimeText", sig.SigningTime?.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            SetText("ReasonText", sig.Reason);
            SetText("LocationText", sig.Location);
            SetText("ContactInfoText", sig.ContactInfo);

            SetText("CertSubjectText", sig.CertificateSubject);
            SetText("CertIssuerText", sig.CertificateIssuer);
            SetText("CertValidFromText", sig.CertificateValidFrom?.ToString("yyyy-MM-dd HH:mm:ss"));
            SetText("CertValidToText", sig.CertificateValidTo?.ToString("yyyy-MM-dd HH:mm:ss"));
            SetText("CertSerialText", sig.CertificateSerialNumber);

            SetText("CoversWholeDocText", sig.CoversWholeDocument ? "Yes — signature covers the entire document" : "No — document may have been modified after signing");
        }

        private void SetText(string controlName, string value)
        {
            TextBlock tb = this.FindControl<TextBlock>(controlName);
            if (tb != null)
            {
                tb.Text = !string.IsNullOrEmpty(value) ? value : "(not provided)";
            }
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
